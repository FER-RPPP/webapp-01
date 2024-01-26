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

    /// <summary>
    /// Controller for managing requirement tasks.
    /// </summary>
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

        /// <summary>
        /// Displays a paginated list of requirement tasks with optional filters.
        /// </summary>
        /// <param name="filter">Filter criteria for tasks.</param>
        /// <param name="page">Page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The view of the index page with filtered requirement tasks.</returns>
        public async Task<IActionResult> Index(RequirementTaskFilter filter, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Get Requirement Tasks called...");
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

        /// <summary>
        /// Displays details of a specific requirement task.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement task.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The details view for a requirement task.</returns>
        public async Task<IActionResult> Details(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Get Requirement Tasks details called...");
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

        /// <summary>
        /// Opens the form for creating a new requirement task.
        /// </summary>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The create view for a requirement task.</returns>
        public IActionResult Create(int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Get Requirement Tasks create page called...");
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

        /// <summary>
        /// Posts the data to create a new requirement task.
        /// </summary>
        /// <param name="requirementTask">The requirement task to create.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the create view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PlannedStartDate,PlannedEndDate,ActualStartDate,ActualEndDate,TaskStatusId,ProjectRequirementId")] RequirementTask requirementTask, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Inserting a Requirement Task...");
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

        /// <summary>
        /// Opens the form for editing an existing requirement task.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement task to edit.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The edit view for a requirement task.</returns>
        public async Task<IActionResult> Edit(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Get Requirement Tasks update page called...");
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

        /// <summary>
        /// Posts the updated data for an existing requirement task.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement task being edited.</param>
        /// <param name="requirementTask">The updated requirement task data.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the edit view.</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,PlannedStartDate,PlannedEndDate,ActualStartDate,ActualEndDate,TaskStatusId,ProjectRequirementId")] RequirementTask requirementTask, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Updating a Requirement Task...");
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

        /// <summary>
        /// Posts the updated data for an existing requirement task using a view model.
        /// </summary>
        /// <param name="taskViewModel">The updated requirement task data in the form of a view model.</param>
        /// <returns>Redirects to the details view if successful, otherwise returns to the edit view with errors.</returns>
        [HttpPost]
        public async Task<IActionResult> EditTask(RequirementTaskViewModel taskViewModel)
        {
            logger.LogInformation("Upading a Requirement Task");
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

        /// <summary>
        /// Opens the form for deleting an existing requirement task.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement task to delete.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The delete view for a requirement task.</returns>
        public async Task<IActionResult> Delete(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Get Requirement Tasks delete page called...");
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

        /// <summary>
        /// Confirms the deletion of a specific requirement task.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement task to delete.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the delete view.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Deleting a Requirement Task...");
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

        /// <summary>
        /// Checks if a requirement task exists in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement task.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>True if the task exists, otherwise false.</returns>
        private bool RequirementTaskExists(Guid id, int page = 1, int sort = 1, bool ascending = true)
         {

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;

                return ctx.RequirementTask.Any(e => e.Id == id);
         }

    }
}
