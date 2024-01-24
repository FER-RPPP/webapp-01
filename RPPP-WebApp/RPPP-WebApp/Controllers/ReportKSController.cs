using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;
using RPPP_WebApp.Model;
using System.Threading.Tasks;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Controller for managing reports related to the Worker entity.
    /// </summary>
    public class ReportKSController : Controller
    {
        private readonly Rppp01Context ctx;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<ReportMMController> logger;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportKSController"/> class.
        /// </summary>
        /// <param name="ctx">The database context.</param>
        /// <param name="environment">The hosting environment.</param>
        /// <param name="logger">The logger.</param>
        public ReportKSController(Rppp01Context ctx, IWebHostEnvironment environment, ILogger<ReportMMController> logger)
        {
            this.ctx = ctx;
            this.environment = environment;
            this.logger = logger;
        }

        /// <summary>
        /// Displays the index view.
        /// </summary>
        /// <returns>The index view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Imports workers from an Excel file.
        /// </summary>
        /// <param name="file">The Excel file containing worker data.</param>
        /// <returns>The result of the import operation.</returns>
        public async Task<IActionResult> ImportWorkers(IFormFile file)
        {
            logger.LogInformation("Importing workers...");
            if (file == null || file.Length == 0)
                return Content("file not selected");

            var workers = new List<Worker>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var workerEmail = worksheet.Cells[row, 2].Value?.ToString();
                        var firstName  = worksheet.Cells[row, 3].Value?.ToString();
                        var lastName = worksheet.Cells[row, 4].Value?.ToString();
                        var phoneNumber = worksheet.Cells[row, 5].Value?.ToString();
                        var organizationName = worksheet.Cells[row, 6].Value?.ToString();

                        var organization = ctx.Organization.FirstOrDefault(w => w.Name == organizationName);
 

                        var worker = new Worker
                        {

                            Id = Guid.NewGuid(),
                            Email = workerEmail,
                            FirstName = firstName,
                            LastName = lastName,
                            PhoneNumber = phoneNumber,
                            Organization = organization
                        };

                        workers.Add(worker);
                    }
                }
            }

            foreach (var worker in workers)
            {
                ctx.Worker.Add(worker);
            }
            await ctx.SaveChangesAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Worker Import Report";
                var worksheet = excel.Workbook.Worksheets.Add("Imported Workers");

                worksheet.Cells[1, 1].Value = "#";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "First Name";
                worksheet.Cells[1, 4].Value = "Last Name";
                worksheet.Cells[1, 5].Value = "Phone Number";
                worksheet.Cells[1, 6].Value = "Organization";
                worksheet.Cells[1, 7].Value = "Import Status";

                int currentRow = 2;
                foreach (var worker in workers)
                {

                    worksheet.Cells[currentRow, 1].Value = currentRow - 1;
                    worksheet.Cells[currentRow, 3].Value = worker.Email;
                    worksheet.Cells[currentRow, 4].Value = worker.FirstName;
                    worksheet.Cells[currentRow, 5].Value = worker.LastName;
                    worksheet.Cells[currentRow, 6].Value = worker.PhoneNumber;
                    worksheet.Cells[currentRow, 7].Value = worker.Organization.Name;
                    worksheet.Cells[currentRow, 8].Value = "Successful";


                    currentRow++;
                }

                worksheet.Cells.AutoFitColumns();

                content = excel.GetAsByteArray();
            }

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ImportedWorkers.xlsx");
        }

        /// <summary>
        /// Exports workers to a PDF file.
        /// </summary>
        /// <returns>A PDF file containing workers or not found response.</returns>
        public async Task<IActionResult> Workers()
        {
            logger.LogInformation("Exporting Workers in PDF...");
            string naslov = "Popis radnika";
            var radnici = await ctx.Worker
                                  .Include(w => w.Organization)
                                  .AsNoTracking()
                                  .OrderBy(o => o.LastName)
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

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(radnici));

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
                    column.PropertyName<Worker>(o => o.Email);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(3);
                    column.HeaderCell("Email", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column => {
                    column.PropertyName<Worker>(o => o.FirstName);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(3);
                    column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column => {
                    column.PropertyName<Worker>(o => o.LastName);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(3);
                    column.HeaderCell("Prezime", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column => {
                    column.PropertyName<Worker>(o => o.PhoneNumber);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(3);
                    column.HeaderCell("Broj telefona", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column => {
                    column.PropertyName<Worker>(o => o.Organization.Name);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5);
                    column.Width(3);
                    column.HeaderCell("Organizacija", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Workers.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Exports workers to an Excel file.
        /// </summary>
        /// <returns>An Excel file containing workers.</returns>
        public async Task<IActionResult> WorkersExcel()
        {
            logger.LogInformation("Exporting Workers in Excel...");
            var workers = await ctx.Worker
                                  .Include(w => w.Organization)
                                  .AsNoTracking()
                                  .OrderBy(o => o.LastName)
                                  .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis radnika";
                var worksheet = excel.Workbook.Worksheets.Add("Popis radnika");

                int currentRow = 1;

                worksheet.Cells[currentRow, 1].Value = "Radnik:";
                worksheet.Cells[currentRow, 2].Value = "Email:";
                worksheet.Cells[currentRow, 3].Value = "Ime";
                worksheet.Cells[currentRow, 4].Value = "Prezime";
                worksheet.Cells[currentRow, 5].Value = "Broj telefona";
                worksheet.Cells[currentRow, 6].Value = "Organizacija";
                currentRow++;

                foreach (var worker in workers)
                {
                    worksheet.Cells[currentRow, 2].Value = worker.Email;
                    worksheet.Cells[currentRow, 3].Value = worker.FirstName;
                    worksheet.Cells[currentRow, 4].Value = worker.LastName;
                    worksheet.Cells[currentRow, 5].Value = worker.PhoneNumber;
                    worksheet.Cells[currentRow, 6].Value = worker.Organization.Name;
                    worksheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    currentRow++;
                }

                worksheet.Cells.AutoFitColumns();


                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "Workers.xlsx");
        }



        /// <summary>
        /// Exports partners to an Excel file.
        /// </summary>
        /// <returns>An Excel file containing partners.</returns>
        public async Task<IActionResult> WorkersByProjectExcel()
        {
            logger.LogInformation("Exporting Workers By Project in Excel...");
            var workers = await ctx.Worker
                                  .Include(w => w.Organization)
                                  .Include(w => w.ProjectPartner)
                                  .ThenInclude(pp => pp.Project)
                                  .Include(w => w.ProjectPartner)
                                  .ThenInclude(pp => pp.Role)
                                  .AsNoTracking()
                                  .OrderBy(o => o.LastName)
                                  .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis suradnika po projektima";
                var worksheet = excel.Workbook.Worksheets.Add("Popis suradnika po projektima");

                int currentRow = 1;

                foreach (var worker in workers)
                {
                    worksheet.Cells[currentRow, 1].Value = "Radnik:";
                    worksheet.Cells[currentRow, 2].Value = "Email:";
                    worksheet.Cells[currentRow, 3].Value = "Ime";
                    worksheet.Cells[currentRow, 4].Value = "Prezime";
                    worksheet.Cells[currentRow, 5].Value = "Broj telefona";
                    worksheet.Cells[currentRow, 6].Value = "Organizacija";
                    currentRow++;

                    worksheet.Cells[currentRow, 2].Value = worker.Email;
                    worksheet.Cells[currentRow, 3].Value = worker.FirstName;
                    worksheet.Cells[currentRow, 4].Value = worker.LastName;
                    worksheet.Cells[currentRow, 5].Value = worker.PhoneNumber;
                    worksheet.Cells[currentRow, 6].Value = worker.Organization.Name;
                    worksheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    currentRow++;

                    worksheet.Cells[currentRow, 1].Value = "Popis suradnji:";
                    currentRow++;

                    worksheet.Cells[currentRow, 1].Value = "#";
                    worksheet.Cells[currentRow, 2].Value = "Projekt";
                    worksheet.Cells[currentRow, 3].Value = "Uloga";
                    worksheet.Cells[currentRow, 4].Value = "Od:";
                    worksheet.Cells[currentRow, 5].Value = "Do:";
                    worksheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    currentRow++;

                    int currentItem = 1;
                    foreach (var pw in worker.ProjectPartner.OrderBy(pp => pp.DateFrom))
                    {
                        worksheet.Cells[currentRow, 1].Value = currentItem++;
                        worksheet.Cells[currentRow, 2].Value = pw.Project.Name;
                        worksheet.Cells[currentRow, 3].Value = pw.Role.Name;
                        worksheet.Cells[currentRow, 4].Value = pw.DateFrom.ToString("dd.MM.yyyy");
                        worksheet.Cells[currentRow, 5].Value = pw.DateTo?.ToString("dd.MM.yyyy");
                        worksheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        currentRow++;
                    }

                }

                worksheet.Cells.AutoFitColumns();
                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "WorkersByProject.xlsx");
        }




        /// <summary>
        /// Creates a PDF report with specified document preferences and main table template.
        /// </summary>
        /// <param name="naslov">The title of the PDF document.</param>
        /// <returns>An instance of the PdfReport class.</returns>
        private PdfReport CreateReport(string naslov)
        {
            var pdf = new PdfReport();

            pdf.DocumentPreferences(doc => {
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "RPPP01",
                    Application = "Firma.MVC Core",
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
    }
}
