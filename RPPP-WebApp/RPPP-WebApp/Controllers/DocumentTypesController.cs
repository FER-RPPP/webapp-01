using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.Views;

namespace RPPP_WebApp.Controllers
{

    /// <summary>
    /// The DocumentTypesController class manages actions related to document types,
    /// including listing, viewing details, creating, editing, and deleting document types.
    /// </summary>
    public class DocumentTypesController : Controller
    {
        private readonly Rppp01Context _context;
        private readonly ILogger<ClientsController> logger;

        public DocumentTypesController(Rppp01Context context, ILogger<ClientsController> logger)
        {
            this.logger = logger;
            _context = context;
        }


        /// <summary>
        /// Displays a paginated list of document types with optional sorting.
        /// </summary>
        /// <param name="sortOrder">Specifies the column to sort by.</param>
        /// <param name="pageNumber">The page number for pagination.</param>
        /// <returns>A view with the list of document types.</returns>
        // GET: DocumentTypes
        public async Task<IActionResult> Index(string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            var documentTypes = from p in _context.DocumentType
                           select p;

            switch (sortOrder)
            {
                case "name_desc":
                    documentTypes = documentTypes.OrderByDescending(p => p.Name);
                    break;
                case "name_asc":
                    documentTypes = documentTypes.OrderBy(p => p.Name);
                    break;
                default:
                    documentTypes = documentTypes.OrderBy(p => p.Name);
                    break;
            }


            int pageSize = 5;
            return View(await PaginatedList<DocumentType>.CreateAsync(documentTypes.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// Shows the details of a specific document type.
        /// </summary>
        /// <param name="id">The unique identifier of the document type.</param>
        /// <returns>A view displaying the document type details if found; otherwise, a NotFound view.</returns>
        // GET: DocumentTypes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.DocumentType == null)
            {
                return NotFound();
            }

            var documentType = await _context.DocumentType
                .FirstOrDefaultAsync(m => m.Id == id);
            if (documentType == null)
            {
                return NotFound();
            }

            return View(documentType);
        }

        /// <summary>
        /// Returns the view for creating a new document type.
        /// </summary>
        /// <returns>A view for document type creation.</returns>
        // GET: DocumentTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Processes the creation of a new document type.
        /// </summary>
        /// <param name="documentType">The document type data to be added.</param>
        /// <returns>A redirect to the Index view if successful; otherwise, returns to the Create view with error messages.</returns>
        // POST: DocumentTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] DocumentType documentType)
        {

            if (!ModelState.IsValid)
            {
                return View(documentType);
            }

            try
            {
                documentType.Id = Guid.NewGuid();
                _context.Add(documentType);
                await _context.SaveChangesAsync();
                logger.LogInformation("Created new document type");
                TempData["StatusMessage"] = "Document type created successfully.";
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
                return View(documentType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(documentType);
            }
        }


        /// <summary>
        /// Returns the edit view for a specific document type.
        /// </summary>
        /// <param name="id">The unique identifier of the document type to be edited.</param>
        /// <returns>An edit view for the specified document type if found; otherwise, a NotFound view.</returns>
        // GET: DocumentTypes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.DocumentType == null)
            {
                return NotFound();
            }

            var documentType = await _context.DocumentType.FindAsync(id);
            if (documentType == null)
            {
                return NotFound();
            }
            return View(documentType);
        }

        /// <summary>
        /// Processes the editing of a specific document type.
        /// </summary>
        /// <param name="id">The unique identifier of the document type being edited.</param>
        /// <param name="documentType">The updated document type data.</param>
        /// <returns>A redirect to the Index view if successful; otherwise, returns to the Edit view with error messages.</returns>
        // POST: DocumentTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name")] DocumentType documentType)
        {

            if (!ModelState.IsValid)
            {
                return View(documentType);
            }

            try
            {
                _context.Update(documentType);
                await _context.SaveChangesAsync();
                logger.LogInformation("Edited document type");
                TempData["StatusMessage"] = "Document type updated successfully.";
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
                return View(documentType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(documentType);
            }
        }


        /// <summary>
        /// Returns the delete confirmation view for a specific document type.
        /// </summary>
        /// <param name="id">The unique identifier of the document type to be deleted.</param>
        /// <returns>A delete confirmation view for the specified document type if found; otherwise, a NotFound view.</returns>
        // GET: DocumentTypes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.DocumentType == null)
            {
                return NotFound();
            }

            var documentType = await _context.DocumentType
                .FirstOrDefaultAsync(m => m.Id == id);
            if (documentType == null)
            {
                return NotFound();
            }

            return View(documentType);
        }

        /// <summary>
        /// Processes the deletion of a specified document type.
        /// </summary>
        /// <param name="id">The unique identifier of the document type to be deleted.</param>
        /// <returns>A redirect to the Index view if successful; otherwise, returns to the Delete view with error messages.</returns>
        // POST: DocumentTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.DocumentType == null)
            {
                return Problem("Entity set 'Rppp01Context.DocumentType'  is null.");
            }
            var documentType = await _context.DocumentType.FindAsync(id);


            if (!ModelState.IsValid)
            {
                return View(documentType);
            }

            try
            {
                if (documentType != null)
                {
                    _context.DocumentType.Remove(documentType);
                }

                await _context.SaveChangesAsync();
                logger.LogInformation("Deleted document type");
                TempData["StatusMessage"] = "Document type deleted successfully.";
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
                return View(documentType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(documentType);
            }
        }

    }
}
