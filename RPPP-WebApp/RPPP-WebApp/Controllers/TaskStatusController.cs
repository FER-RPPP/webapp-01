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

    /// <summary>
    /// Controller for managing task statuses.
    /// </summary>
    public class TaskStatusController : Controller
    {
        private readonly Rppp01Context _context;
        private readonly AppSettings appData;
        private readonly ILogger<TaskStatusController> logger;

        public TaskStatusController(Rppp01Context context, IOptionsSnapshot<AppSettings> options, ILogger<TaskStatusController> logger)
        {
            _context = context;
            appData = options.Value;
            this.logger = logger;
        }

        /// <summary>
        /// Displays a paginated list of task statuses.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The view of the index page with task statuses.</returns>
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Task Status Index called.");
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

        /// <summary>
        /// Displays details of a specific task status.
        /// </summary>
        /// <param name="id">The unique identifier of the task status.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The details view for a task status.</returns>
        public async Task<IActionResult> Details(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Task Status Details called.");
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

        /// <summary>
        /// Opens the form for creating a new task status.
        /// </summary>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The create view for a task status.</returns>
        public IActionResult Create(int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Task Status Create called.");
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            return View();
        }

        /// <summary>
        /// Posts the data to create a new task status.
        /// </summary>
        /// <param name="taskStatus">The task status to create.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the create view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type")] Model.TaskStatus taskStatus, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Inserting new Task Status");
            if (ModelState.IsValid)
            {
                taskStatus.Id = Guid.NewGuid();
                _context.Add(taskStatus);
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
            return View(taskStatus);
        }

        /// <summary>
        /// Opens the form for editing an existing task status.
        /// </summary>
        /// <param name="id">The unique identifier of the task status to edit.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The edit view for a task status.</returns>
        public async Task<IActionResult> Edit(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Task Status Edit page called.");
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

        /// <summary>
        /// Posts the updated data for an existing task status.
        /// </summary>
        /// <param name="id">The unique identifier of the task status being edited.</param>
        /// <param name="taskStatus">The updated task status data.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the edit view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Type")] Model.TaskStatus taskStatus, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Updating Task Status...");
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

        /// <summary>
        /// Opens the form for deleting an existing task status.
        /// </summary>
        /// <param name="id">The unique identifier of the task status to delete.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The delete view for a task status.</returns>
        public async Task<IActionResult> Delete(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Task Status Delete page called.");
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

        /// <summary>
        /// Confirms the deletion of a specific task status.
        /// </summary>
        /// <param name="id">The unique identifier of the task status to delete.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the delete view.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Deleting Task Status...");
            if (_context.TaskStatus == null)
            {
                return Problem("Entity set 'Rppp01Context.TaskStatus'  is null.");
            }
            var taskStatus = await _context.TaskStatus.FindAsync(id);
            if (taskStatus != null)
            {
                _context.TaskStatus.Remove(taskStatus);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unsuccessful deletion: An error occurred while deleting the entity.";
                return View(taskStatus);
            }
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Checks if a task status exists in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the task status.</param>
        /// <returns>True if the task status exists, otherwise false.</returns>
        private bool TaskStatusExists(Guid id)
        {
          return _context.TaskStatus.Any(e => e.Id == id);
        }
    }
}
