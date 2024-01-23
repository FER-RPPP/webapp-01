using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Views;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RPPP_WebApp.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly Rppp01Context _context;
        private readonly ILogger<ProjectsController> logger;


        public ProjectsController(Rppp01Context context, ILogger<ProjectsController> logger)
        {
            _context = context;
            this.logger = logger;
        }

        // GET: Projects
        public async Task<IActionResult> Index(string sortOrder, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["TypeSortParm"] = sortOrder == "Type" ? "type_desc" : "Type";
            ViewData["CurrentFilter"] = searchString;

            var projects = from p in _context.Project
                           .Include(p => p.Card)
                           .Include(p => p.Client)
                           .Include(p => p.Owner)
                           select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                projects = projects.Where(p => p.Name.Contains(searchString)
                                       || p.Type.Contains(searchString)
                                       || p.Name.Contains(searchString)
                                       || p.Owner.Name.Contains(searchString)
                                       || p.Owner.Surname.Contains(searchString)
                                       || p.Owner.Oib.Contains(searchString)
                                       || p.Client.LastName.Contains(searchString)
                                       || p.Client.FirstName.Contains(searchString)
                                       || p.Client.Oib.Contains(searchString)
                                       || p.Card.Iban.Contains(searchString));

            }

            switch (sortOrder)
            {
                case "name_desc":
                    projects = projects.OrderByDescending(p => p.Name);
                    break;
                case "name_asc":
                    projects = projects.OrderBy(p => p.Name);
                    break;
                case "type_desc":
                    projects = projects.OrderByDescending(p => p.Type);
                    break;
                case "type_asc":
                    projects = projects.OrderBy(p => p.Type);
                    break;
                case "iban_desc":
                    projects = projects.OrderByDescending(p => p.Card.Iban);
                    break;
                case "iban_asc":
                    projects = projects.OrderBy(p => p.Card.Iban);
                    break;
                case "client_desc":
                    projects = projects.OrderByDescending(p => p.Client.LastName);
                    break;
                case "client_asc":
                    projects = projects.OrderBy(p => p.Client.LastName);
                    break;
                case "owner_desc":
                    projects = projects.OrderByDescending(p => p.Owner.Surname); 
                    break;
                case "owner_asc":
                    projects = projects.OrderBy(p => p.Owner.Surname);
                    break;
                default:
                    projects = projects.OrderBy(p => p.Name);
                    break;
            }


            int pageSize = 8; 
            return View(await PaginatedList<Project>.CreateAsync(projects.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            await PrepareDropDownLists();

            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Card)
                .Include(p => p.Client)
                .Include(p => p.Owner)
                .Include(p => p.Document)
                    .ThenInclude(d => d.DocumentType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            var query = _context.Document
               .AsNoTracking();

            var document = await query
            .Where(o => o.Project == project)
            .Select(o => new DocumentViewModel
            {
                Id = o.Id,
                Name = o.Name,
                Format = o.Format,
                DocumentType = o.DocumentType.Name
            })
            .ToListAsync();

            var model = new ProjectDocumentsViewModel
            {
                Project = project,
                Document = document
            };

            return View(model);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            ViewData["Card"] = _context.ProjectCard
                .Select(c => new SelectListItem
                {
                    Value = c.Iban.ToString(),
                    Text = $"{c.Iban} ({c.Balance} €)"
                })
                .ToList();

            ViewData["Client"] = _context.Client
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName} ({c.Oib})"
                })
                .ToList();

            ViewData["Owner"] = _context.Owner
                .Select(o => new SelectListItem
                {
                    Value = o.Oib.ToString(),
                    Text = $"{o.Name} {o.Surname} ({o.Oib})"
                })
                .ToList();

            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,Name,Type,OwnerId,CardId")] Project project)
        {

            if (!ModelState.IsValid)
            {
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }

            try
            {
                project.Id = Guid.NewGuid();
                _context.Add(project);
                await _context.SaveChangesAsync();
                logger.LogInformation("Created new project");
                TempData["StatusMessage"] = "Project has been successfully created.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbException ex)
            {
                string errorMessage = "A database update error occurred.";

                if (ex.InnerException is SqlException sqlEx)
                {
                    switch (sqlEx.Number)
                    {
                        case 547: // FK constraint
                            errorMessage = "Operation failed due to a relational constraint. Please ensure all related data is valid.";
                            break;
                        case 2601: // Duplicated key
                        case 2627:
                            errorMessage = "This record already exists. Please ensure the data is unique.";
                            break;
                    }
                }

                TempData["ErrorMessage"] = errorMessage;
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);

            // Assuming Client has properties FirstName, LastName, and Id
            ViewData["ClientId"] = new SelectList(_context.Client.Select(c => new {
                Id = c.Id,
                FullName = $"{c.FirstName} {c.LastName} ({c.Oib})"
            }), "Id", "FullName", project.ClientId);

            // Assuming Owner has properties Name, Surname, and Oib
            ViewData["OwnerId"] = new SelectList(_context.Owner.Select(o => new {
                Oib = o.Oib,
                FullName = $"{o.Name} {o.Surname} ({o.Oib})"
            }), "Oib", "FullName", project.OwnerId);

            return View(project);
        }


        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ClientId,Name,Type,OwnerId,CardId")] Project project)
        {

            if (!ModelState.IsValid)
            {
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }

            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
                logger.LogInformation("Edited project");
                TempData["StatusMessage"] = "Project has been successfully updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                string errorMessage = "A database update error occurred.";

                if (ex.InnerException is SqlException sqlEx)
                {
                    switch (sqlEx.Number)
                    {
                        case 547: // FK constraint
                            errorMessage = "Operation failed due to a relational constraint. Please ensure all related data is valid.";
                            break;
                        case 2601: // Duplicated key
                        case 2627:
                            errorMessage = "This record already exists. Please ensure the data is unique.";
                            break;
                    }
                }

                TempData["ErrorMessage"] = errorMessage;
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Card)
                .Include(p => p.Client)
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Project == null)
            {
                return Problem("Entity set 'Rppp01Context.Project'  is null.");
            }
            var project = await _context.Project.FindAsync(id);

            if (!ModelState.IsValid)
            {
                return View(project);
            }

            try
            {
                if (project != null)
                {
                    _context.Project.Remove(project);
                }
                await _context.SaveChangesAsync();
                logger.LogInformation("Deleted project");
                TempData["StatusMessage"] = "Project has been successfully deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                string errorMessage = "A database update error occurred.";

                if (ex.InnerException is SqlException sqlEx)
                {
                    switch (sqlEx.Number)
                    {
                        case 547: // FK constraint
                            errorMessage = "Operation failed due to a relational constraint. Please ensure all related data is valid.";
                            break;
                        case 2601: // Duplicated key
                        case 2627:
                            errorMessage = "This record already exists. Please ensure the data is unique.";
                            break;
                    }
                }

                TempData["ErrorMessage"] = errorMessage;
                return View(project);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(project);
            }
        }

        private async Task PrepareDropDownLists()
        {
            var documentTypes = await _context.DocumentType
                                  .ToListAsync();


            var documentTypesList = documentTypes.Select(documentType => new SelectListItem
            {
                Text = $"{documentType.Name}",
                Value = documentType.Id.ToString()
            }).ToList();

            ViewBag.DocumentTypes = documentTypesList;
        }

        [HttpGet]
        public async Task<IActionResult> GetDocument(Guid id)
        {
            var document = await _context.Document
                                       .Where(o => o.Id == id)
                                       .Select(o => new DocumentViewModel
                                       {
                                           Id = o.Id,
                                           Name = o.Name,
                                           Format = o.Format,
                                           DocumentType = o.DocumentType.Name,
                                       })
                                       .FirstOrDefaultAsync();

            if (document != null)
            {
                return PartialView(document);
            }
            else
            {
                return NotFound($"Invalid Document Id: {id}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDocument(Guid Id)
        {

            var document = _context.Document.AsNoTracking().Where(o => o.Id == Id).SingleOrDefault();
            if (document != null)
            {
                try
                {
                    _context.Remove(document);
                    await _context.SaveChangesAsync();
                    logger.LogInformation("Deleted projects document");
                    TempData["StatusMessage"] = "Document has been successfully deleted.";
                }
                catch (Exception exc)
                {
                    TempData["StatusMessage"] = "Document has been successfully deleted.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = $"Document with id: {Id} does not exist";
            }

            return PartialView(document);
        }

        [HttpGet]
        public async Task<IActionResult> EditDocument(Guid id)
        {
            var document = await _context.Document
                                       .Where(o => o.Id == id)
                                       .Select(o => new DocumentViewModel
                                       {
                                           Id = o.Id,
                                           Name = o.Name,
                                           Format = o.Format,
                                           DocumentType = o.DocumentType.Name

                                       })
                                       .FirstOrDefaultAsync();

            await PrepareDropDownLists();
            if (document != null)
            {
                return PartialView(document);
            }
            else
            {
                return NotFound($"Invalid Document Id: {id}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditDocument(DocumentViewModel document)
        {
            if (document == null)
            {
                return NotFound("No data sent");
            }
            Document dbDocument = await _context.Document.FindAsync(document.Id);
            if (dbDocument == null)
            {
                return NotFound($"Invalid Document Id: {document.Id}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    dbDocument.Id = document.Id;
                    dbDocument.Name = document.Name;
                    dbDocument.Format = document.Format;
                    Guid documentType = new Guid(document.DocumentType);
                    dbDocument.DocumentTypeId = documentType;
                    await _context.SaveChangesAsync();
                    logger.LogInformation("Edited projects document");
                    return RedirectToAction(nameof(GetDocument), new { id = document.Id });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(document);
                }
            }
            else
            {
                return PartialView(document);
            }
        }


        [HttpPost]
        public async Task<IActionResult> NewDocument(Document document)
        {
            Debug.WriteLine(document.Name + " " + document.Id + " " + document.Format + " " + document.ProjectId + " " + document.DocumentTypeId);

            if (ModelState.IsValid)
            {
                try
                {
                    document.Id = Guid.NewGuid();
                    _context.Add(document);
                    await _context.SaveChangesAsync();
                    logger.LogInformation("Created new projects document");

                    TempData["StatusMessage"] = "Document has been successfully created.";
                }
                catch (Exception exc)
                {
                    TempData["ErrorMessage"] = "There was a problem creating a document.";
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    await PrepareDropDownLists();
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Document is not valid.";

                await PrepareDropDownLists();
            }

            return Content($"<script>setTimeout(function() {{ window.location.href='/rppp/01/Projects/Details/{document.ProjectId}'; }}, 500);</script>", "text/html");
        }
    }

}
