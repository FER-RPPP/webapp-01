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
using Document = RPPP_WebApp.Model.Document;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Manages report generation and data import for projects, documents, and clients.
    /// </summary>
    public class ReportIBController : Controller
    {
        private readonly Rppp01Context ctx;
        private readonly ILogger<ReportIBController> logger;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";


        public ReportIBController(Rppp01Context ctx, ILogger<ReportIBController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
        }

        /// <summary>
        /// Displays the index view of the report controller.
        /// </summary>
        /// <returns>The index view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Imports project data from an Excel file and generates a report.
        /// </summary>
        /// <param name="file">The uploaded Excel file containing project data.</param>
        /// <returns>An asynchronous task that returns a file download result.</returns>
        public async Task<IActionResult> ImportProject(IFormFile file)
        {
            logger.LogInformation("Importing Project.");
            if (file == null || file.Length == 0)
                return Content("File not selected.");

            var projects = new List<Project>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {

                        var name = worksheet.Cells[row, 1].Value?.ToString();
                        var client = worksheet.Cells[row, 2].Value?.ToString();
                        var type = worksheet.Cells[row, 3].Value?.ToString();
                        var iban = worksheet.Cells[row, 4].Value?.ToString();
                        var owner = worksheet.Cells[row, 5].Value?.ToString();

                        string pattern = @"\(([^)]*)\)"; // Regularni izraz za pronalaženje teksta unutar zagrada

                        string clientOib = Regex.Match(client, pattern).Groups[1].Value;
                        string ownerOib = Regex.Match(owner, pattern).Groups[1].Value;

                        var clientDB = ctx.Client.Where(o => o.Oib == clientOib).First();

                        if (!string.IsNullOrEmpty(clientOib) &&
                            !string.IsNullOrEmpty(name) &&
                            !string.IsNullOrEmpty(type) &&
                            !string.IsNullOrEmpty(ownerOib) &&
                            !string.IsNullOrEmpty(iban)
                            )
                        {
                            var project = new Project
                            {
                                Id = Guid.NewGuid(),
                                Name = name,
                                CardId = iban,
                                Type = type,
                                OwnerId = ownerOib,
                                ClientId = clientDB.Id
                            };

                            projects.Add(project);
                        }

                    }
                }
            }

            foreach (var project in projects)
            {
                ctx.Project.Add(project);
            }

            await ctx.SaveChangesAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Project Import Report";
                var worksheet = excel.Workbook.Worksheets.Add("Imported Project");

                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Type";
                worksheet.Cells[1, 3].Value = "Document Name";
                worksheet.Cells[1, 4].Value = "Document Type";

                int currentRow = 2;
                foreach (var project in projects)
                {
                    var document = ctx.Document.Include(o => o.DocumentType).Where(x => x.ProjectId == project.Id);


                    worksheet.Cells[currentRow, 1].Value = project.Name.ToString();
                    worksheet.Cells[currentRow, 2].Value = project.Type;
                    worksheet.Cells[currentRow, 3].Value = document.IsNullOrEmpty() ? "" : document.First().Name;
                    worksheet.Cells[currentRow, 4].Value = document.IsNullOrEmpty() ? "" : document.First().DocumentType.Name;
                    worksheet.Cells[currentRow, 6].Value = "Successful";

                    currentRow++;
                }

                worksheet.Cells.AutoFitColumns();

                content = excel.GetAsByteArray();
            }

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ImportedProjects.xlsx");
        }

        /// <summary>
        /// Generates a PDF report of all clients.
        /// </summary>
        /// <returns>An asynchronous task that returns a PDF file download result.</returns>
        public async Task<IActionResult> Client()
        {
            string naslov = "Clients list";
            var clients = await ctx.Client
                                  .AsNoTracking()
                                  .OrderBy(c => c.LastName)
                                  .ToListAsync();
            PdfReport report = CreateReport(naslov);



            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(clients));

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
                    column.PropertyName<Client>(o => o.Oib);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(3);
                    column.HeaderCell("OIB");
                });

                columns.AddColumn(column => {
                    column.PropertyName<Client>(o => o.FirstName);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(3);
                    column.HeaderCell("First name", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column => {
                    column.PropertyName<Client>(o => o.LastName);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(3);
                    column.HeaderCell("Last name", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column => {
                    column.PropertyName<Client>(o => o.Email);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(3);
                    column.HeaderCell("E-mail", horizontalAlignment: HorizontalAlignment.Center);
                });

            });

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=clients.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Generates a PDF report of all projects.
        /// </summary>
        /// <returns>An asynchronous task that returns a PDF file download result.</returns>
        public async Task<IActionResult> Project()
        {
            string title = "Project List";
            var projects = await ctx.Project
                                  .AsNoTracking()
                                  .Include(p => p.Client)
                                  .OrderBy(o => o.Name)
                                  .ToListAsync();
            PdfReport report = CreateReport(title);

            report.PagesFooter(footer => {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header => {
                header.CacheHeader(cache: true);
                header.DefaultHeader(defaultHeader => {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(title);
                });
            });

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(projects));

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
                    column.PropertyName<Project>(o => o.Name);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(5);
                    column.HeaderCell("Name");
                });

                columns.AddColumn(column => {
                    column.PropertyName<Project>(o => o.Type);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(3);
                    column.HeaderCell("Project Type", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column => {
                    column.PropertyName<Project>(o => o.Client.Oib);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(3);
                    column.HeaderCell("Client (OIB)", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column => {
                    column.PropertyName<Project>(o => o.CardId);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("IBAN", horizontalAlignment: HorizontalAlignment.Center);
                });

            });

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=projects.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Generates a PDF report of all documents.
        /// </summary>
        /// <returns>An asynchronous task that returns a PDF file download result.</returns>
        public async Task<IActionResult> Document()
        {
            string title = "Documents list";
            var documents = await ctx.Document
                                  .AsNoTracking()
                                  .Include(o => o.DocumentType)
                                  .OrderBy(o => o.Name)
                                  .ToListAsync();
            PdfReport report = CreateReport(title);

            report.PagesFooter(footer => {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header => {
                header.CacheHeader(cache: true);
                header.DefaultHeader(defaultHeader => {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(title);
                });
            });

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(documents));

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
                    column.PropertyName<Document>(o => o.Name);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(7);
                    column.HeaderCell("Name", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column => {
                    column.PropertyName<Document>(o => o.Format);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(7);
                    column.HeaderCell("Format", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column => {
                    column.PropertyName<Document>(o => o.DocumentType.Name);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Document Type", horizontalAlignment: HorizontalAlignment.Center);
                });

            });

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=documents.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Generates a PDF report of all document types.
        /// </summary>
        /// <returns>An asynchronous task that returns a PDF file download result.</returns>
        public async Task<IActionResult> DocumentType()
        {
            string title = "Document Type List";
            var types = await ctx.DocumentType
                                  .AsNoTracking()
                                  .OrderBy(o => o.Name)
                                  .ToListAsync();
            PdfReport report = CreateReport(title);

            report.PagesFooter(footer => {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header => {
                header.CacheHeader(cache: true);
                header.DefaultHeader(defaultHeader => {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(title);
                });
            });

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(types));

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
                    column.PropertyName<DocumentType>(o => o.Name);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(3);
                    column.HeaderCell("Type Name", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=document_types.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }


        /// <summary>
        /// Creates a basic PDF report setup with a given title.
        /// </summary>
        /// <param name="naslov">The title of the report.</param>
        /// <returns>A configured PdfReport object.</returns>
        private PdfReport CreateReport(string naslov)
        {
            var pdf = new PdfReport();

            pdf.DocumentPreferences(doc => {
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "RPPP01",
                    Title = naslov
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })
            .MainTableTemplate(template => {
                template.BasicTemplate(BasicTemplate.ProfessionalTemplate);
            })
            .MainTablePreferences(table => {
                table.ColumnsWidthsType(TableColumnWidthType.Relative);
                table.GroupsPreferences(new GroupsPreferences
                {
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

        /// <summary>
        /// Generates a master-detail PDF report for project documents.
        /// </summary>
        /// <returns>An asynchronous task that returns a PDF file download result.</returns>
        public async Task<IActionResult> MdProjectDocuments()
        {
            string title = "Project Documents";
            var documents = await ctx.Document
                                  .AsNoTracking()
                                  .Include(d => d.Project)
                                  .Include(d => d.DocumentType)
                                  .OrderBy(d => d.Project.Name)
                                  .Select(d => new
                                  {
                                      ProjectId = d.ProjectId,
                                      ProjectName = d.Project.Name,
                                      DocumentName = d.Name,
                                      DocumentType = d.DocumentType.Name,
                                      ProjectType = d.Project.Type,
                                      Format = d.Format
                                  }
                                  )
                                  .ToListAsync();

            PdfReport report = CreateReport(title);

            report.PagesFooter(footer => {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header => {
                header.CacheHeader(cache: true);
                header.CustomHeader(new MasterDetailsHeaders(title)
                {
                    PdfRptFont = header.PdfFont
                });
            });

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(documents));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName("ProjectId");
                    column.Group(
                        (val1, val2) =>
                        {
                            return (Guid)val1 == (Guid)val2;
                        });
                });

                columns.AddColumn(column =>
                {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName("DocumentName");
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(7);
                    column.HeaderCell("Document Name", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Document>(o => o.Format);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Format", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Document>(o => o.DocumentType);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(5);
                    column.HeaderCell("Document Type", horizontalAlignment: HorizontalAlignment.Center);
                });


            });

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=documents_per_project.pdf");
                return File(pdf, "application/pdf");
            }
            else
                return NotFound();
        }

        /// <summary>
        /// Custom class for master-details headers in PDF reports.
        /// </summary>
        public class MasterDetailsHeaders : IPageHeader
        {
            private string naslov;
            public MasterDetailsHeaders(string naslov)
            {
                this.naslov = naslov;
            }
            public IPdfFont PdfRptFont { set; get; }

            public PdfGrid RenderingGroupHeader(iTextSharp.text.Document pdfDoc, PdfWriter pdfWriter, IList<CellData> newGroupInfo, IList<SummaryCellData> summaryData)
            {
                var projectName = newGroupInfo.GetSafeStringValueOf(nameof(ReportProjectIBMasterDetail.ProjectName));
                var projectType = newGroupInfo.GetSafeStringValueOf(nameof(ReportProjectIBMasterDetail.ProjectType));


                var table = new PdfGrid(relativeWidths: new[] { 2f, 5f, 2f, 3f }) { WidthPercentage = 100 };

                table.AddSimpleRow(
                    (cellData, cellProperties) => {
                        cellData.Value = "Project name: ";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) => {
                        cellData.Value = projectName;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) => {
                        cellData.Value = "Project Type: ";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) => {
                        cellData.Value = projectType;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    });

                return table.AddBorderToTable(borderColor: BaseColor.LightGray, spacingBefore: 5f);
            }
            public PdfGrid RenderingReportHeader(iTextSharp.text.Document pdfDoc, PdfWriter pdfWriter, IList<SummaryCellData> summaryData)
            {
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


        /// <summary>
        /// Generates an Excel report of all projects.
        /// </summary>
        /// <returns>An asynchronous task that returns an Excel file download result.</returns>
        public async Task<IActionResult> ProjectExcel()
        {
            var project = await ctx.Project
                                  .AsNoTracking()
                                  .Include(o => o.Client)
                                  .Include(o => o.Owner)
                                  .Include(o => o.Card)
                                  .OrderBy(o => o.Name)
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Project List";
                excel.Workbook.Properties.Author = "FER";
                var worksheet = excel.Workbook.Worksheets.Add("Project");

                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Client";
                worksheet.Cells[1, 3].Value = "Type";
                worksheet.Cells[1, 4].Value = "Iban";
                worksheet.Cells[1, 5].Value = "Owner";

                for (int i = 0; i < project.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = project[i].Name;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = project[i].Client.FirstName + " " + project[i].Client.LastName + " (" + project[i].Client.Oib + ")";
                    worksheet.Cells[i + 2, 3].Value = project[i].Type;
                    worksheet.Cells[i + 2, 4].Value = project[i].Card.Iban;
                    worksheet.Cells[i + 2, 5].Value = project[i].Owner.Name + " " + project[i].Owner.Surname + " (" + project[i].Owner.Oib + ")";


                }

                worksheet.Cells[1, 1, project.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "projects.xlsx");
        }

        /// <summary>
        /// Generates an Excel report of all documents.
        /// </summary>
        /// <returns>An asynchronous task that returns an Excel file download result.</returns>
        public async Task<IActionResult> DocumentsExcel()
        {
            var documents = await ctx.Document
                                  .AsNoTracking()
                                  .Include(o => o.DocumentType)
                                  .OrderBy(o => o.Name)
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Documents List";
                excel.Workbook.Properties.Author = "FER";
                var worksheet = excel.Workbook.Worksheets.Add("Document");

                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Format";
                worksheet.Cells[1, 3].Value = "Document Type";

                for (int i = 0; i < documents.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = documents[i].Name;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = documents[i].Format;
                    worksheet.Cells[i + 2, 3].Value = documents[i].DocumentType.Name;
                }

                worksheet.Cells[1, 1, documents.Count + 1, 6].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "documents.xlsx");
        }

        /// <summary>
        /// Generates an Excel report of all document types.
        /// </summary>
        /// <returns>An asynchronous task that returns an Excel file download result.</returns>
        public async Task<IActionResult> DocumentTypeExcel()
        {
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Document types";
                excel.Workbook.Properties.Author = "FER";

                var typeWorksheet = excel.Workbook.Worksheets.Add("DocumentType");
                typeWorksheet.Cells[1, 1].Value = "Type";
                var transactionTypes = await ctx.DocumentType.AsNoTracking().ToListAsync();
                for (int i = 0; i < transactionTypes.Count; i++)
                {
                    typeWorksheet.Cells[i + 2, 1].Value = transactionTypes[i].Name;
                }
                typeWorksheet.Cells[1, 1, transactionTypes.Count + 1, 1].AutoFitColumns();

             
                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "document-types.xlsx");
        }

        /// <summary>
        /// Generates an Excel report of all clients.
        /// </summary>
        /// <returns>An asynchronous task that returns an Excel file download result.</returns>
        public async Task<IActionResult> ClientExcel()
        {
            var clients = await ctx.Client
                                  .AsNoTracking()
                                  .OrderBy(o => o.Oib)
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Client List";
                excel.Workbook.Properties.Author = "FER";
                var worksheet = excel.Workbook.Worksheets.Add("Client");

                worksheet.Cells[1, 1].Value = "OIB";
                worksheet.Cells[1, 2].Value = "First Name";
                worksheet.Cells[1, 3].Value = "Last Name";
                worksheet.Cells[1, 4].Value = "E-mail";

                for (int i = 0; i < clients.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = clients[i].Oib;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = clients[i].FirstName;
                    worksheet.Cells[i + 2, 3].Value = clients[i].LastName;
                    worksheet.Cells[i + 2, 4].Value = clients[i].Email;
                }

                worksheet.Cells[1, 1, clients.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "clients.xlsx");
        }


        /// <summary>
        /// Generates a master-detail Excel report for project documents.
        /// </summary>
        /// <returns>An asynchronous task that returns an Excel file download result.</returns>
        public async Task<IActionResult> MasterDetailExcel()
        {
            var projects = await ctx.Project
                                        .AsNoTracking()
                                        .Include(o => o.Document)
                                        .OrderBy(o => o.Name)
                                        .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Project Documents";
                excel.Workbook.Properties.Author = "FER";
               
                var worksheet = excel.Workbook.Worksheets.Add("Project");
                worksheet.Cells[1, 1].Value = "Project Name";
                worksheet.Cells[1, 2].Value = "Project Type";
                worksheet.Cells[1, 3].Value = "Document Name";
                worksheet.Cells[1, 4].Value = "Document Format";
                worksheet.Cells[1, 5].Value = "Document Type";

                int currentRow = 2;
                foreach (var project in projects)
                {
                    worksheet.Cells[currentRow, 1].Value = project.Name;
                    worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[currentRow, 2].Value = project.Type;

                    int row = currentRow;
                    foreach (var document in ctx.Document.Include(o => o.DocumentType).Where(o => o.ProjectId == project.Id).ToList())
                    {
                        worksheet.Cells[row, 5].Value = document.Name;
                        worksheet.Cells[row, 6].Value = document.Format;
                        worksheet.Cells[row, 7].Value = document.DocumentType.Name;
                        row++;
                    }
                    currentRow = row + 1;
                }

                worksheet.Cells[1, 1, currentRow - 1, 9].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "project-documents.xlsx");
        }
    }
}
