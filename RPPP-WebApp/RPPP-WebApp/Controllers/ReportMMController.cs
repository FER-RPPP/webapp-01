using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PdfRpt.ColumnsItemsTemplates;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;
using RPPP_WebApp.Model;
using System;
using RPPP_WebApp.ViewModels;
using PdfRpt.Core.Helper;

namespace RPPP_WebApp.Controllers
{
    public class ReportMMController : Controller
    {
        private readonly Rppp01Context ctx;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ReportMMController(Rppp01Context ctx, IWebHostEnvironment environment)
        {
            this.ctx = ctx;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RequirementTasksExcel()
        {
            var requirementTasks = await ctx.RequirementTask
                .Include(rt => rt.ProjectRequirement)
                .Include(rt => rt.ProjectWork)
                .Include(rt => rt.ProjectWork.Project)
                .Include(rt => rt.TaskStatus)
                .OrderBy(rt => rt.ProjectRequirement.Id)
                .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Requirement Tasks Report";
                var worksheet = excel.Workbook.Worksheets.Add("Requirement Tasks");

                int currentRow = 1;
                worksheet.Cells[currentRow, 1].Value = "#";
                worksheet.Cells[currentRow, 2].Value = "Id";
                worksheet.Cells[currentRow, 3].Value = "Planned Start Date";
                worksheet.Cells[currentRow, 4].Value = "Planned End Date";
                worksheet.Cells[currentRow, 5].Value = "Actual Start Date";
                worksheet.Cells[currentRow, 6].Value = "Actual End Date";
                worksheet.Cells[currentRow, 7].Value = "Task Status";
                worksheet.Cells[currentRow, 8].Value = "Project Work Title";
                worksheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                currentRow++;

                foreach (var pr in requirementTasks)
                {

                    worksheet.Cells[currentRow, 1].Value = currentRow - 1;
                    worksheet.Cells[currentRow, 2].Value = pr.Id;
                    worksheet.Cells[currentRow, 3].Value = pr.PlannedStartDate.ToString("dd.MM.yyyy");
                    worksheet.Cells[currentRow, 4].Value = pr.PlannedEndDate.ToString("dd.MM.yyyy");
                    worksheet.Cells[currentRow, 5].Value = pr.ActualStartDate?.ToString("dd.MM.yyyy");
                    worksheet.Cells[currentRow, 6].Value = pr.ActualEndDate?.ToString("dd.MM.yyyy");
                    worksheet.Cells[currentRow, 7].Value = pr.TaskStatus.Type;
                    worksheet.Cells[currentRow, 8].Value = pr.ProjectWork.Title;
                    worksheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    currentRow++; 

                

                }

                worksheet.Cells.AutoFitColumns();


                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "RequirementTasks.xlsx");
        }

        public async Task<IActionResult> TaskStatusesExcel()
        {
            var taskStatuses = await ctx.TaskStatus
                .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Task Statuses Report";
                var worksheet = excel.Workbook.Worksheets.Add("Task Statuses");

                int currentRow = 1;
                worksheet.Cells[currentRow, 1].Value = "#";
                worksheet.Cells[currentRow, 2].Value = "Id";
                worksheet.Cells[currentRow, 3].Value = "Type";

                worksheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                currentRow++;

                foreach (var pr in taskStatuses)
                {

                    worksheet.Cells[currentRow, 1].Value = currentRow - 1;
                    worksheet.Cells[currentRow, 2].Value = pr.Id;
                    worksheet.Cells[currentRow, 3].Value = pr.Type;
                  
                    worksheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    currentRow++;
                }

                worksheet.Cells.AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "TaskStatuses.xlsx");
        }

        public async Task<IActionResult> ProjectRequirementsExcel()
        {
            var projectRequirements = await ctx.ProjectRequirement
                        .Include(pr => pr.Project)
                        .Include(pr => pr.RequirementPriority)
                        .Include(pr => pr.RequirementTask)
                            .ThenInclude(rt => rt.TaskStatus)
                        .OrderBy(pr => pr.Type)
                        .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Project Requirements Report";
                var worksheet = excel.Workbook.Worksheets.Add("Project Requirements");

                int currentRow = 1;
                foreach (var pr in projectRequirements)
                {
                    worksheet.Cells[currentRow, 1].Value = "Project Requirement:";
                    worksheet.Cells[currentRow, 2].Value = "Type";
                    worksheet.Cells[currentRow, 3].Value = "Description";
                    worksheet.Cells[currentRow, 4].Value = "Priority";
                    worksheet.Cells[currentRow, 5].Value = "Project Name";
                    currentRow++;

                    worksheet.Cells[currentRow, 2].Value = pr.Type;
                    worksheet.Cells[currentRow, 3].Value = pr.Description;
                    worksheet.Cells[currentRow, 4].Value = pr.RequirementPriority.Type;
                    worksheet.Cells[currentRow, 5].Value = pr.Project.Name;
                    worksheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    currentRow++; // Move to the next row

                    worksheet.Cells[currentRow, 1].Value = "Requirement Tasks:";
                    currentRow++;

                    // Headers for RequirementTask details
                    worksheet.Cells[currentRow, 1].Value = "#";
                    worksheet.Cells[currentRow, 2].Value = "Planned Start Date";
                    worksheet.Cells[currentRow, 3].Value = "Planned End Date";
                    worksheet.Cells[currentRow, 4].Value = "Actual Start Date";
                    worksheet.Cells[currentRow, 5].Value = "Actual End Date";
                    worksheet.Cells[currentRow, 6].Value = "Task Status";
                    worksheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    currentRow++; // Move to the next row

                    int detailCount = 1;
                    foreach (var task in pr.RequirementTask.OrderBy(rt => rt.PlannedStartDate))
                    {
                        // Detail entity - RequirementTask
                        worksheet.Cells[currentRow, 1].Value = detailCount++;
                        worksheet.Cells[currentRow, 2].Value = task.PlannedStartDate.ToString("dd.MM.yyyy");
                        worksheet.Cells[currentRow, 3].Value = task.PlannedEndDate.ToString("dd.MM.yyyy");
                        worksheet.Cells[currentRow, 4].Value = task.ActualStartDate?.ToString("dd.MM.yyyy");
                        worksheet.Cells[currentRow, 5].Value = task.ActualEndDate?.ToString("dd.MM.yyyy");
                        worksheet.Cells[currentRow, 6].Value = task.TaskStatusId.ToString();
                        worksheet.Row(currentRow).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        currentRow++; // Move to the next row
                    }

                    // Add an empty row after each master-detail group
                    currentRow++;
                }

                worksheet.Cells.AutoFitColumns();


                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "ProjectRequirements.xlsx");
        }



        public async Task<IActionResult> ProjectRequirementsPdf()
        {
            string title = "Project Requirements Report";
            var requirementTasks = await ctx.RequirementTask
                                                  .Include(rt => rt.ProjectRequirement)
                                                  .Include(rt => rt.ProjectWork)
                                                  .Include(rt => rt.ProjectWork.Project)
                                                  .Include(rt => rt.TaskStatus)
                                                  .OrderBy(rt => rt.ProjectRequirement.Id)
                                                  .ThenBy(rt => rt.ProjectWork.Title)
                                                  .Select(rt => new
                                                  {
                                                      ProjectRequirementId = rt.ProjectRequirement.Id,
                                                      PlannedStartDate = rt.PlannedStartDate,
                                                      PlannedEndDate = rt.PlannedEndDate,
                                                      ActualStartDate = rt.ActualStartDate,
                                                      ActualEndDate = rt.ActualEndDate,
                                                      TaskStatus = rt.TaskStatus.Type,
                                                      ProjectWorkTitle = rt.ProjectWork.Title,
                                                      ProjectName = rt.ProjectWork.Project.Name
                                                  })
                                                  .ToListAsync();
            PdfReport report = CreateReport(title);

            // Header and Footer setup
            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true); 
                header.CustomHeader(new MasterDetailsHeaders(title)
                {
                    PdfRptFont = header.PdfFont
                });
            });

            // Data Source and Columns setup
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(requirementTasks));


            report.MainTableColumns(columns =>
            {
                // Stupac po kojem se grupira
                columns.AddColumn(column => {
                    column.PropertyName("ProjectRequirementId");
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
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                }); 
                
                columns.AddColumn(column =>
                {
                    column.PropertyName<RequirementTaskDenorm>(rt => rt.PlannedStartDate);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Planned Start Date", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<RequirementTaskDenorm>(rt => rt.PlannedEndDate);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Planned End Date", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<RequirementTaskDenorm>(rt => rt.ActualStartDate);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(1);
                    column.HeaderCell("Actual Start Date", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<RequirementTaskDenorm>(rt => rt.ActualEndDate);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5);
                    column.Width(1);
                    column.HeaderCell("Actual End Date", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<RequirementTaskDenorm>(rt => rt.TaskStatus);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(6);
                    column.Width(1);
                    column.HeaderCell("Task Status", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<RequirementTaskDenorm>(rt => rt.ProjectWorkTitle);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(7);
                    column.Width(1);
                    column.HeaderCell("Project Work Title", horizontalAlignment: HorizontalAlignment.Center);
                });


            });





            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=project_requirements_report.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }


        private PdfReport CreateReport(string naslov)
        {
            var pdf = new PdfReport();

            pdf.DocumentPreferences(doc =>
            {
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "FER-ZPR",
                    Application = "Firma.MVC Core",
                    Title = naslov
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })

            //
            .MainTableTemplate(template =>
            {
                template.BasicTemplate(BasicTemplate.ProfessionalTemplate);
            })
            .MainTablePreferences(table =>
            {
                table.ColumnsWidthsType(TableColumnWidthType.Relative);
                //table.NumberOfDataRowsPerPage(20);
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
            var projectRequirementId = newGroupInfo.GetSafeStringValueOf(nameof(RequirementTaskDenorm.ProjectRequirementId));
            var projectName = newGroupInfo.GetSafeStringValueOf(nameof(RequirementTaskDenorm.ProjectName));

            var table = new PdfGrid(relativeWidths: new[] { 2f, 5f, 2f, 3f }) { WidthPercentage = 100 };

            table.AddSimpleRow(
                (cellData, cellProperties) =>
                {
                    cellData.Value = "Requirement Id:";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.TableRowData = newGroupInfo;
                    var cellTemplate = new HyperlinkField(BaseColor.Black, false)
                    {
                        TextPropertyName = nameof(RequirementTaskDenorm.ProjectRequirementId),
                        NavigationUrlPropertyName = nameof(RequirementTaskDenorm.ProjectRequirementId),
                        BasicProperties = new CellBasicProperties
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            PdfFontStyle = DocumentFontStyle.Bold,
                            PdfFont = PdfRptFont
                        }
                    };

                    cellData.CellTemplate = cellTemplate;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = "Project Name";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = projectName;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                });


            return table.AddBorderToTable(borderColor: BaseColor.LightGray, spacingBefore: 5f);
        }



        public PdfGrid RenderingReportHeader(iTextSharp.text.Document pdfDoc, PdfWriter pdfWriter, IList<SummaryCellData> summaryData)
        {
            var table = new PdfGrid(numColumns: 1) { WidthPercentage = 100 };
            table.AddSimpleRow(
               (cellData, cellProperties) =>
               {
                   cellData.Value = naslov;
                   cellProperties.PdfFont = PdfRptFont;
                   cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                   cellProperties.HorizontalAlignment = HorizontalAlignment.Center;
               });
            return table.AddBorderToTable();
        }


    }
}
