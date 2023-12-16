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
    public class RequirementTasksController : Controller
    {
        private readonly Rppp01Context ctx;
        private readonly ILogger<RequirementTasksController> logger;
        private readonly AppSettings appData;

        public RequirementTasksController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<RequirementTasksController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        // GET: RequirementTasks
        public async Task<IActionResult> Index(RequirementTaskFilter filter, int page = 1, int sort = 1, bool ascending = true)
        {

            int pagesize = appData.PageSize;
            var query = ctx.RequirementTask
                           .AsNoTracking();

            string errorMessage = "Ne postoji niti jedan Zadatak";
            if (!string.IsNullOrEmpty(filter.ProjectWorkTitle))
            {
                query = query.Where(p => p.ProjectWork.Title.Contains(filter.ProjectWorkTitle));
                errorMessage += $" gdje je opis: {filter.ProjectWorkTitle}";
            }

            if (!string.IsNullOrEmpty(filter.TaskStatus))
            {
                query = query.Where(p => p.TaskStatus.Type.Contains(filter.TaskStatus));
                errorMessage += $" gdje je status: {filter.TaskStatus}";
            }

            int count = await query.CountAsync();
            if (count == 0)
            {
                logger.LogInformation(errorMessage);
                TempData[Constants.Message] = errorMessage;
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index));
            }

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

            var requirementTask = await query
                        .Select(o => new RequirementTaskViewModel
                        {
                            Id = o.Id,
                            PlannedStartDate = o.PlannedStartDate,
                            PlannedEndDate = o.PlannedEndDate,
                            ActualStartDate = o.ActualStartDate,
                            ActualEndDate = o.ActualEndDate,
                            TaskStatus = o.TaskStatus.Type,
                            ProjectWork = o.ProjectWork.Title,
                            RequirementDescription = o.ProjectRequirement.Description
                        })
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToListAsync();

            var model = new RequirementTasksViewModel
            {
                RequirementTasks = requirementTask,
                PagingInfo = pagingInfo,
                Filter = filter
            };

            return View(model);
        }

        // GET: RequirementTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || ctx.RequirementTask == null)
            {
                return NotFound();
            }

            var requirementTask = await ctx.RequirementTask
                .Include(r => r.ProjectRequirement)
                .Include(r => r.ProjectWork)
                .Include(r => r.TaskStatus)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requirementTask == null)
            {
                return NotFound();
            }

            return View(requirementTask);
        }

        // GET: RequirementTasks/Create
        public IActionResult Create()
        {
            ViewData["ProjectRequirementId"] = new SelectList(ctx.ProjectRequirement, "Id", "Description");
            ViewData["Id"] = new SelectList(ctx.ProjectWork, "Id", "Title");
            ViewData["TaskStatusId"] = new SelectList(ctx.TaskStatus, "Id", "Type");
            return View();
        }

        // POST: RequirementTasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PlannedStartDate,PlannedEndDate,ActualStartDate,ActualEndDate,TaskStatusId,ProjectRequirementId")] RequirementTask requirementTask)
        {
            if (ModelState.IsValid)
            {
                ctx.Add(requirementTask);
                await ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectRequirementId"] = new SelectList(ctx.ProjectRequirement, "Id", "Description", requirementTask.ProjectRequirementId);
            ViewData["Id"] = new SelectList(ctx.ProjectWork, "Id", "Title", requirementTask.Id);
            ViewData["TaskStatusId"] = new SelectList(ctx.TaskStatus, "Id", "Type", requirementTask.TaskStatusId);
            return View(requirementTask);
        }

        // GET: RequirementTasks/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || ctx.RequirementTask == null)
            {
                return NotFound();
            }

            var requirementTask = await ctx.RequirementTask.FindAsync(id);
            if (requirementTask == null)
            {
                return NotFound();
            }
            ViewData["ProjectRequirementId"] = new SelectList(ctx.ProjectRequirement, "Id", "Description", requirementTask.ProjectRequirementId);
            ViewData["Id"] = new SelectList(ctx.ProjectWork, "Id", "Title", requirementTask.Id);
            ViewData["TaskStatusId"] = new SelectList(ctx.TaskStatus, "Id", "Type", requirementTask.TaskStatusId);
            return View(requirementTask);
        }

        // POST: RequirementTasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,PlannedStartDate,PlannedEndDate,ActualStartDate,ActualEndDate,TaskStatusId,ProjectRequirementId")] RequirementTask requirementTask)
        {
            if (id != requirementTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(requirementTask);
                    await ctx.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequirementTaskExists(requirementTask.Id))
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
            ViewData["ProjectRequirementId"] = new SelectList(ctx.ProjectRequirement, "Id", "Description", requirementTask.ProjectRequirementId);
            ViewData["Id"] = new SelectList(ctx.ProjectWork, "Id", "Title", requirementTask.Id);
            ViewData["TaskStatusId"] = new SelectList(ctx.TaskStatus, "Id", "Type", requirementTask.TaskStatusId);
            return View(requirementTask);
        }

        // GET: RequirementTasks/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || ctx.RequirementTask == null)
            {
                return NotFound();
            }

            var requirementTask = await ctx.RequirementTask
                .Include(r => r.ProjectRequirement)
                .Include(r => r.ProjectWork)
                .Include(r => r.TaskStatus)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requirementTask == null)
            {
                return NotFound();
            }

            return View(requirementTask);
        }

        // POST: RequirementTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (ctx.RequirementTask == null)
            {
                return Problem("Entity set 'Rppp01Context.RequirementTask'  is null.");
            }
            var requirementTask = await ctx.RequirementTask.FindAsync(id);
            if (requirementTask != null)
            {
                ctx.RequirementTask.Remove(requirementTask);
            }
            
            await ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RequirementTaskExists(Guid id)
        {
          return ctx.RequirementTask.Any(e => e.Id == id);
        }
    }
}
