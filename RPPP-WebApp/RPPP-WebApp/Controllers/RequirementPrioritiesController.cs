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
    public class RequirementPrioritiesController : Controller
    {
        private readonly Rppp01Context _context;
        private readonly AppSettings appData;

        public RequirementPrioritiesController(Rppp01Context context, IOptionsSnapshot<AppSettings> options)
        {
            _context = context;
            appData = options.Value;
        }

        // GET: RequirementPriorities
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {

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

        // GET: RequirementPriorities/Details/5
        public async Task<IActionResult> Details(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
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

        // GET: RequirementPriorities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RequirementPriorities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type")] RequirementPriority requirementPriority, int page = 1, int sort = 1, bool ascending = true)
        {
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

        // GET: RequirementPriorities/Edit/5
        public async Task<IActionResult> Edit(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
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

        // POST: RequirementPriorities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Type")] RequirementPriority requirementPriority, int page = 1, int sort = 1, bool ascending = true)
        {
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

        // GET: RequirementPriorities/Delete/5
        public async Task<IActionResult> Delete(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
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

        // POST: RequirementPriorities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
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

        private bool RequirementPriorityExists(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
          return _context.RequirementPriority.Any(e => e.Id == id);
        }
    }
}
