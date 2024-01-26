using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog.Filters;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{

    /// <summary>
    /// Controller for managing requirement priorities.
    /// </summary>
    public class RequirementPrioritiesController : Controller
    {
        private readonly Rppp01Context _context;
        private readonly AppSettings appData;
        private readonly ILogger<RequirementPrioritiesController> logger;

        public RequirementPrioritiesController(Rppp01Context context, IOptionsSnapshot<AppSettings> options, ILogger<RequirementPrioritiesController> logger)
        {
            _context = context;
            appData = options.Value;
            this.logger = logger;
        }

        /// <summary>
        /// Displays a paginated list of requirement priorities.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The view of the index page with requirement priorities.</returns>
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Get Requirement Priorities called...");
            var query = _context.RequirementPriority
                           .AsNoTracking();

            int pagesize = appData.PageSize;
            int count = await query.CountAsync();
            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var requirementPriorities = await query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            var model = new RequirementPrioritiesViewModel
            {
                RequirementPriorities = requirementPriorities,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        /// <summary>
        /// Displays details of a specific requirement priority.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement priority.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The details view for a requirement priority.</returns>
        public async Task<IActionResult> Details(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Get Requirement Priorities details called...");
            if (id == null || _context.RequirementPriority == null)
            {
                return NotFound();
            }

            var requirementPriority = await _context.RequirementPriority
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requirementPriority == null)
            {
                return NotFound();
            }

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(requirementPriority);
        }

        /// <summary>
        /// Opens the form for creating a new requirement priority.
        /// </summary>
        /// <returns>The create view for a requirement priority.</returns>
        public IActionResult Create()
        {
            logger.LogInformation("Get Requirement Priorities create page called...");
            return View();
        }

        /// <summary>
        /// Posts the data to create a new requirement priority.
        /// </summary>
        /// <param name="requirementPriority">The requirement priority to create.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the create view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type")] RequirementPriority requirementPriority, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Inserting a Requirement Prioritiy...");
            if (ModelState.IsValid)
            {
                requirementPriority.Id = Guid.NewGuid();
                _context.Add(requirementPriority);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Unsuccessful add: An error occurred while saving the entity changes.";
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(requirementPriority);
        }

        /// <summary>
        /// Opens the form for editing an existing requirement priority.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement priority to edit.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The edit view for a requirement priority.</returns>
        public async Task<IActionResult> Edit(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Get Requirement Priorities Edit page called...");
            if (id == null || _context.RequirementPriority == null)
            {
                return NotFound();
            }

            var requirementPriority = await _context.RequirementPriority.FindAsync(id);
            if (requirementPriority == null)
            {
                return NotFound();
            }

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(requirementPriority);
        }

        /// <summary>
        /// Posts the updated data for an existing requirement priority.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement priority being edited.</param>
        /// <param name="requirementPriority">The updated requirement priority data.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the edit view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Type")] RequirementPriority requirementPriority, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Updating Requirement Prioritiy...");
            if (id != requirementPriority.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(requirementPriority);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequirementPriorityExists(requirementPriority.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(requirementPriority);
        }

        /// <summary>
        /// Opens the form for deleting an existing requirement priority.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement priority to delete.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The delete view for a requirement priority.</returns>
        public async Task<IActionResult> Delete(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Get Requirement Priorities Delete page called...");
            if (id == null || _context.RequirementPriority == null)
            {
                return NotFound();
            }

            var requirementPriority = await _context.RequirementPriority
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requirementPriority == null)
            {
                return NotFound();
            }

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(requirementPriority);
        }

        /// <summary>
        /// Confirms the deletion of a specific requirement priority.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement priority to delete.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the delete view.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Deleting Requirement Prioritiy...");
            if (_context.RequirementPriority == null)
            {
                return Problem("Entity set 'Rppp01Context.RequirementPriority'  is null.");
            }
            var requirementPriority = await _context.RequirementPriority.FindAsync(id);
            if (requirementPriority != null)
            {
                _context.RequirementPriority.Remove(requirementPriority);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unsuccessful deletion: An error occurred while deleting the entity.";
                return View(requirementPriority);
            }
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Checks if a requirement priority exists in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement priority.</param>
        /// <returns>True if the requirement priority exists, otherwise false.</returns>
        private bool RequirementPriorityExists(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
          return _context.RequirementPriority.Any(e => e.Id == id);
        }
    }
}
