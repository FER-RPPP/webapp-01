using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
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

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(model);
        }

        // GET: RequirementTasks/Details/5
        public async Task<IActionResult> Details(Guid? id, int page = 1, int sort = 1, bool ascending = true)
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

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(requirementTask);
        }

        // GET: RequirementTasks/Create
        public IActionResult Create(int page = 1, int sort = 1, bool ascending = true)
        {
            ViewData["ProjectRequirementId"] = new SelectList(ctx.ProjectRequirement, "Id", "Description");
            ViewData["Id"] = new SelectList(ctx.ProjectWork
                .Select(pw => new
                {
                    Id = pw.Id,
                    Description = pw.Title + " - " + pw.Project.Name
                }),
                "Id",
                "Description");
            ViewData["TaskStatusId"] = new SelectList(ctx.TaskStatus, "Id", "Type");

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View();
        }

        // POST: RequirementTasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PlannedStartDate,PlannedEndDate,ActualStartDate,ActualEndDate,TaskStatusId,ProjectRequirementId")] RequirementTask requirementTask, int page = 1, int sort = 1, bool ascending = true)
        {
            if (ModelState.IsValid)
            {
                ctx.Add(requirementTask);
                try
                {
                    await ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                } catch(Exception ex)
                {
                    TempData["ErrorMessage"] = "Unsuccessful add: An error occurred while saving the entity changes." +
                        "Maybe the ProjectWork item already has it's RequirementTask entity!";

                }
            }
            ViewData["ProjectRequirementId"] = new SelectList(ctx.ProjectRequirement, "Id", "Description", requirementTask.ProjectRequirementId);
            ViewData["Id"] = new SelectList(ctx.ProjectWork, "Id", "Title", requirementTask.Id);
            ViewData["TaskStatusId"] = new SelectList(ctx.TaskStatus, "Id", "Type", requirementTask.TaskStatusId);

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(requirementTask);
        }

        // GET: RequirementTasks/Edit/5
        public async Task<IActionResult> Edit(Guid? id, int page = 1, int sort = 1, bool ascending = true)
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

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(requirementTask);
        }

        // POST: RequirementTasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,PlannedStartDate,PlannedEndDate,ActualStartDate,ActualEndDate,TaskStatusId,ProjectRequirementId")] RequirementTask requirementTask, int page = 1, int sort = 1, bool ascending = true)
        {
           
            if (id != requirementTask.Id)
            {
                return NotFound();
            }
            logger.LogInformation("HERE");
            logger.LogInformation(requirementTask.ToString());
            Console.WriteLine("Hello World!");
            
            if (ModelState.IsValid)
            {
                logger.LogInformation("IT IS VALID!");
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

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(requirementTask);
        }

        [HttpPost]
        public async Task<IActionResult> EditTask(RequirementTaskViewModel taskViewModel)
        {
            logger.LogInformation("HERE");
            if (taskViewModel == null)
            {
                return NotFound("Task data not provided.");
            }



            var dbTask = await ctx.RequirementTask.FindAsync(taskViewModel.Id);
            logger.LogInformation("Got it from db!");
            if (dbTask == null)
            {
                logger.LogInformation("NOT FOUND!");
                return NotFound($"Invalid task ID: {taskViewModel.Id}");
            }

            logger.LogInformation("Checking if it'l be valid");
            if (ModelState.IsValid)
            {
                logger.LogInformation("IT IS VALID!");
                try
                {
                    dbTask.Id = taskViewModel.Id;
                    dbTask.PlannedStartDate = taskViewModel.PlannedStartDate;
                    dbTask.PlannedEndDate = taskViewModel.PlannedEndDate;
                    dbTask.ActualStartDate = taskViewModel.ActualStartDate;
                    dbTask.ActualEndDate = taskViewModel.ActualEndDate;
                    // Add other properties as needed

                    await ctx.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = taskViewModel.Id }); // Redirect to the details page of the parent ProjectRequirement
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the task: " + exc.Message); // Replace with your error handling
                    return View(taskViewModel); // You might need to adjust this return statement based on your view structure
                }
            }
            else
            {
                return View(taskViewModel); // Return the view with validation errors
            }
        }

        // GET: RequirementTasks/Delete/5
        public async Task<IActionResult> Delete(Guid? id, int page = 1, int sort = 1, bool ascending = true)
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

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(requirementTask);
        }

        // POST: RequirementTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, int page = 1, int sort = 1, bool ascending = true)
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

            try
            {
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unsuccessful deletion: An error occurred while deleting the entity.";
                return View(requirementTask);
            }

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return RedirectToAction(nameof(Index));
        }

        

        private bool RequirementTaskExists(Guid id, int page = 1, int sort = 1, bool ascending = true)
         {

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;

                return ctx.RequirementTask.Any(e => e.Id == id);
         }

    }
}
