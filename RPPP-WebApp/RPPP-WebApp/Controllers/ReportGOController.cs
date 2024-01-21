using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfRpt.ColumnsItemsTemplates;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions;
using System.Security.Cryptography;
using System.Globalization;

namespace RPPP_WebApp.Controllers {
  public class ReportGOController : Controller {
    private readonly Rppp01Context ctx;
    private readonly IWebHostEnvironment environment;
    private readonly ILogger<ReportGOController> logger;
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ReportGOController(Rppp01Context ctx, IWebHostEnvironment environment, ILogger<ReportGOController> logger) {
      this.ctx = ctx;
      this.environment = environment;
      this.logger = logger;
    }

    public IActionResult Index() {
      return View();
    }

    public async Task<IActionResult> ImportProjectCard(IFormFile file) {
      logger.LogInformation("Importing Project Card.");
      if (file == null || file.Length == 0)
        return Content("File not selected.");

      var projectCards = new List<ProjectCard>();

      using (var stream = new MemoryStream()) {
        await file.CopyToAsync(stream);
        using (var package = new ExcelPackage(stream)) {
          ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
          int rowCount = worksheet.Dimension.Rows;

          for (int row = 2; row <= rowCount; row++) {

            var iban = worksheet.Cells[row, 1].Value?.ToString();
            var oib = worksheet.Cells[row, 2].Value?.ToString().Split("(")[1].Split(")")[0];
            var balance = decimal.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out var balanceValue) ? balanceValue : 0.0m;
            var activationDateString = worksheet.Cells[row, 4].Value?.ToString();
            var activationDate = DateTime.TryParseExact(
                activationDateString,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var activationDateValue
            )
                ? activationDateValue
                : DateTime.MinValue;

            if (!string.IsNullOrEmpty(iban) &&  
                !string.IsNullOrEmpty(oib)
                ) {
              var projectCard = new ProjectCard {
                Iban = iban,
                Balance = balance,
                Oib = oib,
                ActivationDate = activationDate,
              };

              projectCards.Add(projectCard);
            }
            
          }
        }
      }

      foreach (var projectCard in projectCards) {
        ctx.ProjectCard.Add(projectCard);
      }

      await ctx.SaveChangesAsync();

      byte[] content;
      using (ExcelPackage excel = new ExcelPackage()) {
        excel.Workbook.Properties.Title = "Project Card Import Report";
        var worksheet = excel.Workbook.Worksheets.Add("Imported Project Cards");

        worksheet.Cells[1, 1].Value = "Iban";
        worksheet.Cells[1, 2].Value = "Owner";
        worksheet.Cells[1, 3].Value = "Balance (€)";
        worksheet.Cells[1, 4].Value = "ActivationDate";

        int currentRow = 2;
        foreach (var projectCard in projectCards) {
          worksheet.Cells[currentRow, 1].Value = projectCard.Iban.ToString();
          var owner = ctx.Owner.FirstOrDefault(o => o.Oib == projectCard.Oib);
          worksheet.Cells[currentRow, 2].Value = owner.Name + " " + owner.Surname + " (" + owner.Oib + ")";
          worksheet.Cells[currentRow, 3].Value = projectCard.Balance;
          worksheet.Cells[currentRow, 4].Value = projectCard.ActivationDate.ToString("dd.MM.yyyy");
          worksheet.Cells[currentRow, 5].Value = "Successful";

          currentRow++;
        }

        worksheet.Cells.AutoFitColumns();

        content = excel.GetAsByteArray();
      }

      return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ImportedProjectCards.xlsx");
    }

    public async Task<IActionResult> Owner() {
      string naslov = "Popis vlasnika";
      var vlasnici = await ctx.Owner
                            .AsNoTracking()
                            .OrderBy(o => o.Name)
                            .ToListAsync();
      PdfReport report = CreateReport(naslov);

      report.PagesFooter(footer => {
        footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
      })
      .PagesHeader(header => {
        header.CacheHeader(cache: true);
        header.DefaultHeader(defaultHeader => {
          defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
          defaultHeader.Message(naslov);
        });
      });

      report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(vlasnici));

      report.MainTableColumns(columns => {
        columns.AddColumn(column => {
          column.IsRowNumber(true);
          column.CellsHorizontalAlignment(HorizontalAlignment.Right);
          column.IsVisible(true);
          column.Order(0);
          column.Width(1);
          column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
        });

        columns.AddColumn(column => {
          column.PropertyName<Owner>(o => o.Oib);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(1);
          column.Width(3);
          column.HeaderCell("OIB");
        });

        columns.AddColumn(column => {
          column.PropertyName<Owner>(o => o.Name);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(2);
          column.Width(3);
          column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Center);
        });

        columns.AddColumn(column => {
          column.PropertyName<Owner>(o => o.Surname);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(3);
          column.Width(3);
          column.HeaderCell("Prezime", horizontalAlignment: HorizontalAlignment.Center);
        });
      });

      byte[] pdf = report.GenerateAsByteArray();

      if (pdf != null) {
        Response.Headers.Add("content-disposition", "attachment; filename=vlasnici.pdf");
        return File(pdf, "application/pdf");
      }
      else {
        return NotFound();
      }
    }

    public async Task<IActionResult> ProjectCard() {
      string naslov = "Popis projektnih kartica";
      var projektneKartice = await ctx.ProjectCard
                            .AsNoTracking()
                            .OrderBy(o => o.Iban)
                            .ToListAsync();
      PdfReport report = CreateReport(naslov);

      report.PagesFooter(footer => {
        footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
      })
      .PagesHeader(header => {
        header.CacheHeader(cache: true);
        header.DefaultHeader(defaultHeader => {
          defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
          defaultHeader.Message(naslov);
        });
      });

      report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(projektneKartice));

      report.MainTableColumns(columns => {
        columns.AddColumn(column => {
          column.IsRowNumber(true);
          column.CellsHorizontalAlignment(HorizontalAlignment.Right);
          column.IsVisible(true);
          column.Order(0);
          column.Width(1);
          column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
        });

        columns.AddColumn(column => {
          column.PropertyName<ProjectCard>(o => o.Iban);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(1);
          column.Width(5);
          column.HeaderCell("IBAN");
        });

        columns.AddColumn(column => {
          column.PropertyName<ProjectCard>(o => o.Oib);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(2);
          column.Width(3);
          column.HeaderCell("Vlasnik (OIB)", horizontalAlignment: HorizontalAlignment.Center);
        });

        columns.AddColumn(column => {
          column.PropertyName<ProjectCard>(o => o.Balance);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(3);
          column.Width(2);
          column.HeaderCell("Saldo (€)", horizontalAlignment: HorizontalAlignment.Center);
        });

        columns.AddColumn(column => {
          column.PropertyName<ProjectCard>(o => o.ActivationDate);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(4);
          column.Width(4);
          column.HeaderCell("Datum aktivacije", horizontalAlignment: HorizontalAlignment.Center);
        });
      });

      byte[] pdf = report.GenerateAsByteArray();

      if (pdf != null) {
        Response.Headers.Add("content-disposition", "attachment; filename=projektne-kartice.pdf");
        return File(pdf, "application/pdf");
      }
      else {
        return NotFound();
      }
    }

    public async Task<IActionResult> Transaction() {
      string naslov = "Popis transakcija";
      var transakcije = await ctx.Transaction
                            .AsNoTracking()
                            .OrderBy(o => o.Iban)
                            .Select(o => new TransactionViewModel {
                              Id = o.Id,
                              Iban = o.IbanNavigation.Iban,
                              Recipient = o.Recipient,
                              Amount = o.Amount,
                              Date = o.Date,
                              Type = o.Type.TypeName,
                              Purpose = o.Purpose.PurposeName,
                            })
                            .ToListAsync();
      PdfReport report = CreateReport(naslov);

      report.PagesFooter(footer => {
        footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
      })
      .PagesHeader(header => {
        header.CacheHeader(cache: true);
        header.DefaultHeader(defaultHeader => {
          defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
          defaultHeader.Message(naslov);
        });
      });

      report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(transakcije));

      report.MainTableColumns(columns => {
        columns.AddColumn(column => {
          column.IsRowNumber(true);
          column.CellsHorizontalAlignment(HorizontalAlignment.Right);
          column.IsVisible(true);
          column.Order(0);
          column.Width(1);
          column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
        });

        columns.AddColumn(column => {
          column.PropertyName<Transaction>(o => o.Iban);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(1);
          column.Width(7);
          column.HeaderCell("Pošiljatelj (IBAN)", horizontalAlignment: HorizontalAlignment.Center);
        });

        columns.AddColumn(column => {
          column.PropertyName<Transaction>(o => o.Recipient);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(2);
          column.Width(7);
          column.HeaderCell("Primatelj (IBAN)", horizontalAlignment: HorizontalAlignment.Center);
        });

        columns.AddColumn(column => {
          column.PropertyName<Transaction>(o => o.Amount);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(3);
          column.Width(2);
          column.HeaderCell("Iznos (€)", horizontalAlignment: HorizontalAlignment.Center);
        });

        columns.AddColumn(column => {
          column.PropertyName<Transaction>(o => o.Date);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(4);
          column.Width(5);
          column.HeaderCell("Datum", horizontalAlignment: HorizontalAlignment.Center);
        });

        columns.AddColumn(column => {
          column.PropertyName<Transaction>(o => o.Type);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(5);
          column.Width(2);
          column.HeaderCell("Vrsta", horizontalAlignment: HorizontalAlignment.Center);
        });

        columns.AddColumn(column => {
          column.PropertyName<Transaction>(o => o.Purpose);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(6);
          column.Width(2);
          column.HeaderCell("Svrha", horizontalAlignment: HorizontalAlignment.Center);
        });

      });

      byte[] pdf = report.GenerateAsByteArray();

      if (pdf != null) {
        Response.Headers.Add("content-disposition", "attachment; filename=transakcije.pdf");
        return File(pdf, "application/pdf");
      }
      else {
        return NotFound();
      }
    }

    public async Task<IActionResult> TransactionType() {
      string naslov = "Popis vrsta transakcija";
      var vrste = await ctx.TransactionType
                            .AsNoTracking()
                            .OrderBy(o => o.TypeName)
                            .ToListAsync();
      PdfReport report = CreateReport(naslov);

      report.PagesFooter(footer => {
        footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
      })
      .PagesHeader(header => {
        header.CacheHeader(cache: true);
        header.DefaultHeader(defaultHeader => {
          defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
          defaultHeader.Message(naslov);
        });
      });

      report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(vrste));

      report.MainTableColumns(columns => {
        columns.AddColumn(column => {
          column.IsRowNumber(true);
          column.CellsHorizontalAlignment(HorizontalAlignment.Right);
          column.IsVisible(true);
          column.Order(0);
          column.Width(1);
          column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
        });

        columns.AddColumn(column => {
          column.PropertyName<TransactionType>(o => o.TypeName);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(1);
          column.Width(3);
          column.HeaderCell("Vrsta", horizontalAlignment: HorizontalAlignment.Center);
        });
      });

      byte[] pdf = report.GenerateAsByteArray();

      if (pdf != null) {
        Response.Headers.Add("content-disposition", "attachment; filename=vrste.pdf");
        return File(pdf, "application/pdf");
      }
      else {
        return NotFound();
      }
    }

    public async Task<IActionResult> TransactionPurpose() {
      string naslov = "Popis svrha transakcija";
      var svrhe = await ctx.TransactionPurpose
                            .AsNoTracking()
                            .OrderBy(o => o.PurposeName)
                            .ToListAsync();
      PdfReport report = CreateReport(naslov);

      report.PagesFooter(footer => {
        footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
      })
      .PagesHeader(header => {
        header.CacheHeader(cache: true);
        header.DefaultHeader(defaultHeader => {
          defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
          defaultHeader.Message(naslov);
        });
      });

      report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(svrhe));

      report.MainTableColumns(columns => {
        columns.AddColumn(column => {
          column.IsRowNumber(true);
          column.CellsHorizontalAlignment(HorizontalAlignment.Right);
          column.IsVisible(true);
          column.Order(0);
          column.Width(1);
          column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
        });

        columns.AddColumn(column => {
          column.PropertyName<TransactionPurpose>(o => o.PurposeName);
          column.CellsHorizontalAlignment(HorizontalAlignment.Center);
          column.IsVisible(true);
          column.Order(1);
          column.Width(3);
          column.HeaderCell("Svrha", horizontalAlignment: HorizontalAlignment.Center);
        });
      });

      byte[] pdf = report.GenerateAsByteArray();

      if (pdf != null) {
        Response.Headers.Add("content-disposition", "attachment; filename=svrhe.pdf");
        return File(pdf, "application/pdf");
      }
      else {
        return NotFound();
      }
    }

    private PdfReport CreateReport(string naslov) {
      var pdf = new PdfReport();

      pdf.DocumentPreferences(doc => {
        doc.Orientation(PageOrientation.Portrait);
        doc.PageSize(PdfPageSize.A4);
        doc.DocumentMetadata(new DocumentMetadata {
          Author = "RPPP01",
          Title = naslov
        });
        doc.Compression(new CompressionSettings {
          EnableCompression = true,
          EnableFullCompression = true
        });
      })
      .MainTableTemplate(template => {
        template.BasicTemplate(BasicTemplate.ProfessionalTemplate);
      })
      .MainTablePreferences(table => {
        table.ColumnsWidthsType(TableColumnWidthType.Relative);
        table.GroupsPreferences(new GroupsPreferences {
          GroupType = GroupType.HideGroupingColumns,
          RepeatHeaderRowPerGroup = true,
          ShowOneGroupPerPage = true,
          SpacingBeforeAllGroupsSummary = 5f,
          NewGroupAvailableSpacingThreshold = 150,
          SpacingAfterAllGroupsSummary = 5f
        });
        table.SpacingAfter(4f);
      });

      return pdf;
    }

    public async Task<IActionResult> MasterDetailCardTransactions() {
      string naslov = "Transakcije projektnih kartica";
      var stavke = await ctx.Transaction
                            .AsNoTracking()
                            .Join(ctx.ProjectCard,
                              transaction => transaction.Iban,
                              projectCard => projectCard.Iban,
                              (transaction, projectCard) => new { transaction, projectCard })
                            .Join(ctx.Owner, 
                              combined => combined.projectCard.Oib,
                              owner => owner.Oib,
                              (combined, owner) => new CardTransactionsViewModel
                              {
                                Id = combined.transaction.Id,
                                Iban = combined.transaction.IbanNavigation.Iban,
                                Oib = combined.projectCard.Oib,
                                Balance = combined.projectCard.Balance,
                                ActivationDate = combined.projectCard.ActivationDate,
                                Recipient = combined.transaction.Recipient,
                                Amount = combined.transaction.Amount,
                                Date = combined.transaction.Date,
                                Type = combined.transaction.Type.TypeName,
                                Purpose = combined.transaction.Purpose.PurposeName,
                                Owner = owner.Name + " " + owner.Surname + " (" + combined.projectCard.Oib + ")"
                              })
                            .OrderBy(o => o.Iban)
                            .ToListAsync();
      PdfReport report = CreateReport(naslov);

      report.PagesFooter(footer => {
        footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
      })
      .PagesHeader(header => {
        header.CacheHeader(cache: true);
        header.CustomHeader(new MasterDetailsHeaders(naslov) {
          PdfRptFont = header.PdfFont
        });
      });

      report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(stavke));

      report.MainTableColumns(columns => {
        columns.AddColumn(column => {
          column.PropertyName<Transaction>(o => o.Iban);
          column.Group(
              (val1, val2) => {
                return (string)val1 == (string)val2;
              });
        });
        
          columns.AddColumn(column => {
            column.IsRowNumber(true);
            column.CellsHorizontalAlignment(HorizontalAlignment.Right);
            column.IsVisible(true);
            column.Order(0);
            column.Width(1);
            column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
          });

          columns.AddColumn(column => {
            column.PropertyName<Transaction>(o => o.Iban);
            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
            column.IsVisible(true);
            column.Order(1);
            column.Width(7);
            column.HeaderCell("Pošiljatelj (IBAN)", horizontalAlignment: HorizontalAlignment.Center);
          });

          columns.AddColumn(column => {
            column.PropertyName<Transaction>(o => o.Recipient);
            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
            column.IsVisible(true);
            column.Order(2);
            column.Width(7);
            column.HeaderCell("Primatelj (IBAN)", horizontalAlignment: HorizontalAlignment.Center);
          });

          columns.AddColumn(column => {
            column.PropertyName<Transaction>(o => o.Amount);
            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
            column.IsVisible(true);
            column.Order(3);
            column.Width(2);
            column.HeaderCell("Iznos (€)", horizontalAlignment: HorizontalAlignment.Center);
          });

          columns.AddColumn(column => {
            column.PropertyName<Transaction>(o => o.Date);
            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
            column.IsVisible(true);
            column.Order(4);
            column.Width(5);
            column.HeaderCell("Datum", horizontalAlignment: HorizontalAlignment.Center);
          });

          columns.AddColumn(column => {
            column.PropertyName<Transaction>(o => o.Type);
            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
            column.IsVisible(true);
            column.Order(5);
            column.Width(2);
            column.HeaderCell("Vrsta", horizontalAlignment: HorizontalAlignment.Center);
          });

          columns.AddColumn(column => {
            column.PropertyName<Transaction>(o => o.Purpose);
            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
            column.IsVisible(true);
            column.Order(6);
            column.Width(2);
            column.HeaderCell("Svrha", horizontalAlignment: HorizontalAlignment.Center);
          });

      });

      byte[] pdf = report.GenerateAsByteArray();

      if (pdf != null) {
        Response.Headers.Add("content-disposition", "attachment; filename=transakcije-po-projektnim-karticama.pdf");
        return File(pdf, "application/pdf");
      }
      else
        return NotFound();
    }

    public class MasterDetailsHeaders : IPageHeader {
      private string naslov;
      public MasterDetailsHeaders(string naslov) {
        this.naslov = naslov;
      }
      public IPdfFont PdfRptFont { set; get; }

      public PdfGrid RenderingGroupHeader(iTextSharp.text.Document pdfDoc, PdfWriter pdfWriter, IList<CellData> newGroupInfo, IList<SummaryCellData> summaryData) {
        var iban = newGroupInfo.GetSafeStringValueOf(nameof(RPPP_WebApp.Model.ProjectCard.Iban));
        var oib = newGroupInfo.GetValueOf(nameof(RPPP_WebApp.Model.ProjectCard.Oib));
        var balance = newGroupInfo.GetValueOf(nameof(RPPP_WebApp.Model.ProjectCard.Balance));
        var date = newGroupInfo.GetValueOf(nameof(RPPP_WebApp.Model.ProjectCard.ActivationDate));
        var owner = newGroupInfo.GetSafeStringValueOf(nameof(CardTransactionsViewModel.Owner));

        var table = new PdfGrid(relativeWidths: new[] { 2f, 5f, 2f, 3f }) { WidthPercentage = 100 };

        table.AddSimpleRow(
            (cellData, cellProperties) => {
              cellData.Value = "Iban projektne kartice: ";
              cellProperties.PdfFont = PdfRptFont;
              cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
              cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
            },
            (cellData, cellProperties) => {
              cellData.Value = iban;
              cellProperties.PdfFont = PdfRptFont;
              cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
            },
            (cellData, cellProperties) => {
              cellData.Value = "Datum aktivacije: ";
              cellProperties.PdfFont = PdfRptFont;
              cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
              cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
            },
            (cellData, cellProperties) => {
              cellData.Value = date;
              cellProperties.PdfFont = PdfRptFont;
              cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
            });

        table.AddSimpleRow(
            (cellData, cellProperties) => {
              cellData.Value = "Vlasnik: ";
              cellProperties.PdfFont = PdfRptFont;
              cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
              cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
            },
            (cellData, cellProperties) => {
              cellData.Value = owner;
              cellProperties.PdfFont = PdfRptFont;
              cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
            },
            (cellData, cellProperties) => {
              cellData.Value = "Saldo (€): ";
              cellProperties.PdfFont = PdfRptFont;
              cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
              cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
            },
            (cellData, cellProperties) => {
              cellData.Value = balance;
              cellProperties.PdfFont = PdfRptFont;
              cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
            });

        return table.AddBorderToTable(borderColor: BaseColor.LightGray, spacingBefore: 5f);
      }
      public PdfGrid RenderingReportHeader(iTextSharp.text.Document pdfDoc, PdfWriter pdfWriter, IList<SummaryCellData> summaryData) {
        var table = new PdfGrid(numColumns: 1) { WidthPercentage = 100 };
        table.AddSimpleRow(
           (cellData, cellProperties) => {
             cellData.Value = naslov;
             cellProperties.PdfFont = PdfRptFont;
             cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
             cellProperties.HorizontalAlignment = HorizontalAlignment.Center;
           });
        return table.AddBorderToTable();
      }

    }



    public async Task<IActionResult> ProjectCardExcel() {
      var projectCard = await ctx.ProjectCard
                            .AsNoTracking()
                            .OrderBy(o => o.Iban)
                            .ToListAsync();
      byte[] content;
      using (ExcelPackage excel = new ExcelPackage()) {
        excel.Workbook.Properties.Title = "Popis projektnih kartica";
        excel.Workbook.Properties.Author = "FER";
        var worksheet = excel.Workbook.Worksheets.Add("ProjectCard");

        worksheet.Cells[1, 1].Value = "IBAN";
        worksheet.Cells[1, 2].Value = "Vlasnik";
        worksheet.Cells[1, 3].Value = "Saldo (€)";
        worksheet.Cells[1, 4].Value = "Datum aktivacije";

        for (int i = 0; i < projectCard.Count; i++) {
          worksheet.Cells[i + 2, 1].Value = projectCard[i].Iban;
          worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
          var owner = ctx.Owner.FirstOrDefault(o => o.Oib == projectCard[i].Oib);
          worksheet.Cells[i + 2, 2].Value = owner.Name + " " + owner.Surname + " (" + owner.Oib + ")";
          worksheet.Cells[i + 2, 3].Value = projectCard[i].Balance;
          worksheet.Cells[i + 2, 4].Value = projectCard[i].ActivationDate.ToString("dd.MM.yyyy");
        }

        worksheet.Cells[1, 1, projectCard.Count + 1, 4].AutoFitColumns();

        content = excel.GetAsByteArray();
      }
      return File(content, ExcelContentType, "projekne-kartice.xlsx");
    }

    public async Task<IActionResult> TransactionsExcel() {
      var transaction = await ctx.Transaction
                            .AsNoTracking()
                            .OrderBy(o => o.Iban)
                            .ToListAsync();
      byte[] content;
      using (ExcelPackage excel = new ExcelPackage()) {
        excel.Workbook.Properties.Title = "Popis transakcija";
        excel.Workbook.Properties.Author = "FER";
        var worksheet = excel.Workbook.Worksheets.Add("Transaction");

        worksheet.Cells[1, 1].Value = "Pošiljatelj (IBAN)";
        worksheet.Cells[1, 2].Value = "Primatelj (IBAN)";
        worksheet.Cells[1, 3].Value = "Iznos (€)";
        worksheet.Cells[1, 4].Value = "Datum";
        worksheet.Cells[1, 5].Value = "Vrsta";
        worksheet.Cells[1, 6].Value = "Svrha";

        for (int i = 0; i < transaction.Count; i++) {
          worksheet.Cells[i + 2, 1].Value = transaction[i].Iban;
          worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
          worksheet.Cells[i + 2, 2].Value = transaction[i].Recipient;
          worksheet.Cells[i + 2, 3].Value = transaction[i].Amount;
          worksheet.Cells[i + 2, 4].Value = transaction[i].Date.ToLongDateString() + ",  " + transaction[i].Date.ToLongTimeString();
          var transactionType = ctx.TransactionType.FirstOrDefault(o => o.Id == transaction[i].TypeId);
          worksheet.Cells[i + 2, 5].Value = transactionType.TypeName;
          var transactionPurpose = ctx.TransactionPurpose.FirstOrDefault(o => o.Id == transaction[i].PurposeId);
          worksheet.Cells[i + 2, 6].Value = transactionPurpose.PurposeName;
        }

        worksheet.Cells[1, 1, transaction.Count + 1, 6].AutoFitColumns();

        content = excel.GetAsByteArray();
      }
      return File(content, ExcelContentType, "transakcije.xlsx");
    }

    public async Task<IActionResult> TypesAndPurposesExcel() {
      byte[] content;
      using (ExcelPackage excel = new ExcelPackage()) {
        excel.Workbook.Properties.Title = "Popis transakcija";
        excel.Workbook.Properties.Author = "FER";

        var typeWorksheet = excel.Workbook.Worksheets.Add("TransactionType");
        typeWorksheet.Cells[1, 1].Value = "Vrsta";
        var transactionTypes = await ctx.TransactionType.AsNoTracking().ToListAsync();
        for (int i = 0; i < transactionTypes.Count; i++) {
          typeWorksheet.Cells[i + 2, 1].Value = transactionTypes[i].TypeName;
        }
        typeWorksheet.Cells[1, 1, transactionTypes.Count + 1, 1].AutoFitColumns();

        var purposeWorksheet = excel.Workbook.Worksheets.Add("TransactionPurpose");
        purposeWorksheet.Cells[1, 1].Value = "Svrha";
        var transactionPurpose = await ctx.TransactionPurpose.AsNoTracking().ToListAsync();
        for (int i = 0; i < transactionTypes.Count; i++) {
          purposeWorksheet.Cells[i + 2, 1].Value = transactionPurpose[i].PurposeName;
        }
        purposeWorksheet.Cells[1, 1, transactionPurpose.Count + 1, 1].AutoFitColumns();

        content = excel.GetAsByteArray();
      }
      return File(content, ExcelContentType, "vrste-i-svrhe.xlsx");
    }

    public async Task<IActionResult> OwnersExcel() {
      var owner = await ctx.Owner
                            .AsNoTracking()
                            .OrderBy(o => o.Oib)
                            .ToListAsync();
      byte[] content;
      using (ExcelPackage excel = new ExcelPackage()) {
        excel.Workbook.Properties.Title = "Popis vlasnika";
        excel.Workbook.Properties.Author = "FER";
        var worksheet = excel.Workbook.Worksheets.Add("Owner");

        worksheet.Cells[1, 1].Value = "OIB";
        worksheet.Cells[1, 2].Value = "Name";
        worksheet.Cells[1, 3].Value = "Surname";

        for (int i = 0; i < owner.Count; i++) {
          worksheet.Cells[i + 2, 1].Value = owner[i].Oib;
          worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
          worksheet.Cells[i + 2, 2].Value = owner[i].Name;
          worksheet.Cells[i + 2, 3].Value = owner[i].Surname;

        }

        worksheet.Cells[1, 1, owner.Count + 1, 3].AutoFitColumns();

        content = excel.GetAsByteArray();
      }
      return File(content, ExcelContentType, "vlasnici.xlsx");
    }

    public async Task<IActionResult> MasterDetailExcel() {
      var projectCards = await ctx.ProjectCard
                                  .AsNoTracking()
                                  .OrderBy(o => o.Iban)
                                  .ToListAsync();

      byte[] content;
      using (ExcelPackage excel = new ExcelPackage()) {
        excel.Workbook.Properties.Title = "Popis projektnih kartica i njihovih transakcija";
        excel.Workbook.Properties.Author = "FER";

        var worksheet = excel.Workbook.Worksheets.Add("ProjectCard");
        worksheet.Cells[1, 1].Value = "IBAN projektne kartice";
        worksheet.Cells[1, 2].Value = "Vlasnik projektne kartice";
        worksheet.Cells[1, 3].Value = "Saldo (€) projektne kartice";
        worksheet.Cells[1, 4].Value = "Datum aktivacije projektne kartice";
        worksheet.Cells[1, 5].Value = "Primatelj (IBAN)";
        worksheet.Cells[1, 6].Value = "Iznos (€) transakcije";
        worksheet.Cells[1, 7].Value = "Datum transakcije";
        worksheet.Cells[1, 8].Value = "Vrsta transakcije";
        worksheet.Cells[1, 9].Value = "Svrha transakcije";

        int currentRow = 2;
        foreach (var projectCard in projectCards) {
          worksheet.Cells[currentRow, 1].Value = projectCard.Iban;
          worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
          var owner = ctx.Owner.FirstOrDefault(o => o.Oib == projectCard.Oib);
          worksheet.Cells[currentRow, 2].Value = owner.Name + " " + owner.Surname + " (" + owner.Oib + ")";
          worksheet.Cells[currentRow, 3].Value = projectCard.Balance;
          worksheet.Cells[currentRow, 4].Value = projectCard.ActivationDate.ToLongDateString();

          int row = currentRow;
          foreach (var transaction in ctx.Transaction.Where(o => o.Iban == projectCard.Iban).ToList()) {
            worksheet.Cells[row, 5].Value = transaction.Recipient;
            worksheet.Cells[row, 6].Value = transaction.Amount;
            worksheet.Cells[row, 7].Value = transaction.Date.ToLongDateString() + ",  " + transaction.Date.ToLongTimeString();
            var transactionType = ctx.TransactionType.FirstOrDefault(o => o.Id == transaction.TypeId);
            worksheet.Cells[row, 8].Value = transactionType.TypeName;
            var transactionPurpose = ctx.TransactionPurpose.FirstOrDefault(o => o.Id == transaction.PurposeId);
            worksheet.Cells[row, 9].Value = transactionPurpose.PurposeName;
            row++;
          }
          currentRow = row + 1;
        }

        worksheet.Cells[1, 1, currentRow - 1, 9].AutoFitColumns();

        content = excel.GetAsByteArray();
      }
      return File(content, ExcelContentType, "projekne-kartice-i-njihove-transakcije.xlsx");
    }
  }
}
