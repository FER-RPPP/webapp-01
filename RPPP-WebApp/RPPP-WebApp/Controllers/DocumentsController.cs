using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.Views;
using Microsoft.Data.SqlClient;
using System.Data.Common;


namespace RPPP_WebApp.Controllers
{

    /// <summary>
    /// The DocumentsController class is responsible for managing actions related to documents,
    /// including listing, viewing details, creating, editing, and deleting documents.
    /// </summary>
    public class DocumentsController : Controller
    {
        private readonly Rppp01Context _context;
        private readonly ILogger<ClientsController> logger;


        public DocumentsController(Rppp01Context context, ILogger<ClientsController> logger)
        {
            this.logger = logger;
            _context = context;
        }

        /// <summary>
        /// The DocumentsController class is responsible for managing actions related to documents,
        /// including listing, viewing details, creating, editing, and deleting documents.
        /// </summary>
        // GET: Documents
        public async Task<IActionResult> Index(string sortOrder, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["FormatSortParm"] = sortOrder == "Format" ? "format_desc" : "Format";
            ViewData["DocumentTypeSortParm"] = sortOrder == "DocumentType" ? "doctype_desc" : "DocumentType";
            ViewData["ProjectNameSortParm"] = sortOrder == "ProjectName" ? "projectname_desc" : "ProjectName";
            ViewData["CurrentFilter"] = searchString;

            var documentsQuery = _context.Document
                                        .Include(d => d.DocumentType)
                                        .Include(d => d.Project)
                                        .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                documentsQuery = documentsQuery.Where(d => d.Name.Contains(searchString)
                                                            || d.Format.Contains(searchString)
                                                            || d.DocumentType.Name.Contains(searchString)
                                                            || d.Project.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    documentsQuery = documentsQuery.OrderByDescending(d => d.Name);
                    break;
                case "Name":
                    documentsQuery = documentsQuery.OrderBy(d => d.Name);
                    break;
                case "format_desc":
                    documentsQuery = documentsQuery.OrderByDescending(d => d.Format);
                    break;
                case "Format":
                    documentsQuery = documentsQuery.OrderBy(d => d.Format);
                    break;
                case "doctype_desc":
                    documentsQuery = documentsQuery.OrderByDescending(d => d.DocumentType.Name);
                    break;
                case "DocumentType":
                    documentsQuery = documentsQuery.OrderBy(d => d.DocumentType.Name);
                    break;
                case "projectname_desc":
                    documentsQuery = documentsQuery.OrderByDescending(d => d.Project.Name);
                    break;
                case "ProjectName":
                    documentsQuery = documentsQuery.OrderBy(d => d.Project.Name);
                    break;
                default:
                    documentsQuery = documentsQuery.OrderBy(d => d.Name);
                    break;
            }

            int pageSize = 5;
            return View(await PaginatedList<Document>.CreateAsync(documentsQuery.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        /// <summary>
        /// Shows the details of a specific document.
        /// </summary>
        /// <param name="id">The unique identifier of the document.</param>
        /// <returns>A view displaying the document details if found; otherwise, a NotFound view.</returns>
        // GET: Documents/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Document == null)
            {
                return NotFound();
            }

            var document = await _context.Document
                .Include(d => d.DocumentType)
                .Include(d => d.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        /// <summary>
        /// Returns the view for creating a new document.
        /// </summary>
        /// <returns>A view for document creation.</returns>
        // GET: Documents/Create
        public IActionResult Create()
        {
            ViewData["DocumentTypeId"] = new SelectList(_context.DocumentType, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
            return View();
        }

        /// <summary>
        /// Processes the creation of a new document.
        /// </summary>
        /// <param name="document">The document data to be added.</param>
        /// <returns>A redirect to the Index view if successful; otherwise, returns to the Create view with error messages.</returns>
        // POST: Documents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Format,ProjectId,DocumentTypeId")] Document document)
        {
            if (!ModelState.IsValid)
            {
                ViewData["DocumentTypeId"] = new SelectList(_context.DocumentType, "Id", "Name");
                ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
                return View(document);
            }

            try
            {
                document.Id = Guid.NewGuid();
                _context.Add(document);
                await _context.SaveChangesAsync();
                logger.LogInformation("Created new document");
                TempData["StatusMessage"] = "Document has been successfully added.";
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
                ViewData["DocumentTypeId"] = new SelectList(_context.DocumentType, "Id", "Name");
                ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
                return View(document);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                ViewData["DocumentTypeId"] = new SelectList(_context.DocumentType, "Id", "Name");
                ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
                return View(document);
            }
        }


        /// <summary>
        /// Returns the edit view for a specific document.
        /// </summary>
        /// <param name="id">The unique identifier of the document to be edited.</param>
        /// <returns>An edit view for the specified document if found; otherwise, a NotFound view.</returns>
        // GET: Documents/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Document == null)
            {
                return NotFound();
            }

            var document = await _context.Document.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            ViewData["DocumentTypeId"] = new SelectList(_context.DocumentType, "Id", "Name", document.DocumentTypeId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "CardId", document.ProjectId);
            return View(document);
        }


        /// <summary>
        /// Processes the editing of a specific document.
        /// </summary>
        /// <param name="id">The unique identifier of the document being edited.</param>
        /// <param name="document">The updated document data.</param>
        /// <returns>A redirect to the Index view if successful; otherwise, returns to the Edit view with error messages.</returns>
        // POST: Documents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Format,ProjectId,DocumentTypeId")] Document document)
        {

            if (!ModelState.IsValid)
            {
                ViewData["DocumentTypeId"] = new SelectList(_context.DocumentType, "Id", "Name");
                ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
                return View(document);
            }

            try
            {
                _context.Update(document);
                await _context.SaveChangesAsync();
                logger.LogInformation("Edited document");
                TempData["StatusMessage"] = "Document has been successfully updated.";
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
                ViewData["DocumentTypeId"] = new SelectList(_context.DocumentType, "Id", "Name");
                ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
                return View(document);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                ViewData["DocumentTypeId"] = new SelectList(_context.DocumentType, "Id", "Name");
                ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
                return View(document);
            }
        }


        /// <summary>
        /// Returns the delete confirmation view for a specific document.
        /// </summary>
        /// <param name="id">The unique identifier of the document to be deleted.</param>
        /// <returns>A delete confirmation view for the specified document if found; otherwise, a NotFound view.</returns>
        // GET: Documents/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Document == null)
            {
                return NotFound();
            }

            var document = await _context.Document
                .Include(d => d.DocumentType)
                .Include(d => d.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }


        /// <summary>
        /// Processes the deletion of a specified document.
        /// </summary>
        /// <param name="id">The unique identifier of the document to be deleted.</param>
        /// <returns>A redirect to the Index view if successful; otherwise, returns to the Delete view with error messages.</returns>
        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Document == null)
            {
                TempData["StatusMessage"] = "Entity set 'Rppp01Context.Document'  is null.";
                return Problem(nameof(Document));
            }
            var document = await _context.Document.FindAsync(id);


            if (!ModelState.IsValid)
            {
                return View(document);
            }

            try
            {
                if (document != null)
                {
                    _context.Document.Remove(document);
                }
                await _context.SaveChangesAsync();
                logger.LogInformation("Deleted document");
                TempData["StatusMessage"] = "Document has been successfully deleted.";
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
                return View(document);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(document);
            }
        }


        /// <summary>
        /// Checks if a document exists in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the document.</param>
        /// <returns>True if the document exists; otherwise, false.</returns>
        private bool DocumentExists(Guid id)
        {
          return _context.Document.Any(e => e.Id == id);
        }
    }
}
