using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    public class TaskStatusController : Controller
    {
        private readonly Rppp01Context _context;
        private readonly AppSettings appData;

        public TaskStatusController(Rppp01Context context, IOptionsSnapshot<AppSettings> options)
        {
            _context = context;
            appData = options.Value;
        }

        // GET: TaskStatus
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            var query = _context.TaskStatus
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

            var taskStatuses = await query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            var model = new TaskStatusesViewModel
            {
                TaskStatuses = taskStatuses,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        // GET: TaskStatus/Details/5
        public async Task<IActionResult> Details(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            if (id == null || _context.TaskStatus == null)
            {
                return NotFound();
            }

            var taskStatus = await _context.TaskStatus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskStatus == null)
            {
                return NotFound();
            }
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(taskStatus);
        }

        // GET: TaskStatus/Create
        public IActionResult Create(int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            return View();
        }

        // POST: TaskStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type")] Model.TaskStatus taskStatus, int page = 1, int sort = 1, bool ascending = true)
        {
            if (ModelState.IsValid)
            {
                taskStatus.Id = Guid.NewGuid();
                _context.Add(taskStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            return View(taskStatus);
        }

        // GET: TaskStatus/Edit/5
        public async Task<IActionResult> Edit(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            if (id == null || _context.TaskStatus == null)
            {
                return NotFound();
            }

            var taskStatus = await _context.TaskStatus.FindAsync(id);
            if (taskStatus == null)
            {
                return NotFound();
            }
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            return View(taskStatus);
        }

        // POST: TaskStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Type")] Model.TaskStatus taskStatus, int page = 1, int sort = 1, bool ascending = true)
        {
            if (id != taskStatus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskStatusExists(taskStatus.Id))
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
            return View(taskStatus);
        }

        // GET: TaskStatus/Delete/5
        public async Task<IActionResult> Delete(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            if (id == null || _context.TaskStatus == null)
            {
                return NotFound();
            }

            var taskStatus = await _context.TaskStatus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskStatus == null)
            {
                return NotFound();
            }
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            return View(taskStatus);
        }

        // POST: TaskStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
            if (_context.TaskStatus == null)
            {
                return Problem("Entity set 'Rppp01Context.TaskStatus'  is null.");
            }
            var taskStatus = await _context.TaskStatus.FindAsync(id);
            if (taskStatus != null)
            {
                _context.TaskStatus.Remove(taskStatus);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskStatusExists(Guid id)
        {
          return _context.TaskStatus.Any(e => e.Id == id);
        }
    }
}
