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
    public class DocumentTypesController : Controller
    {
        private readonly Rppp01Context _context;
        private readonly ILogger<ClientsController> logger;


        public DocumentTypesController(Rppp01Context context, ILogger<ClientsController> logger)
        {
            this.logger = logger;
            _context = context;
        }

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

        // GET: DocumentTypes/Create
        public IActionResult Create()
        {
            return View();
        }

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
