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
    /// Controller for managing project requirements.
    /// </summary>
    public class ProjectRequirementsController : Controller
    {
        private readonly Rppp01Context ctx;
        private readonly ILogger<ProjectRequirementsController> logger;
        private readonly AppSettings appData;

        public ProjectRequirementsController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjectRequirementsController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        /// <summary>
        /// Displays a paginated list of project requirements.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The view of the index page with project requirements.</returns>
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("GET Project Requirements called...");
            var query = ctx.ProjectRequirement
                .Include(p => p.RequirementTask).ThenInclude(p => p.ProjectWork)
                .Include(p => p.Project)
                .Include(p => p.RequirementPriority)
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

            var projectRequirements = await query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            var model = new ProjectRequirementsViewModel
            {
                ProjectRequirement = projectRequirements,
                PagingInfo = pagingInfo
            };

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(model);
        }

        /// <summary>
        /// Displays details of a specific project requirement.
        /// </summary>
        /// <param name="id">The unique identifier of the project requirement.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The details view for a project requirement.</returns>
        public async Task<IActionResult> Details(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("GET Project Requirements Details called...");
            if (id == null || ctx.ProjectRequirement == null)
            {
                return NotFound();
            }

            var projectRequirement = await ctx.ProjectRequirement
                .Include(p => p.Project)
                .Include(p => p.RequirementPriority)
                .Include(p => p.RequirementTask)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectRequirement == null)
            {
                return NotFound();
            }

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(projectRequirement);
        }

        /// <summary>
        /// Opens the form for creating a new project requirement.
        /// </summary>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The create view for a project requirement.</returns>
        public IActionResult Create(int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("GET Project Requirements Create called...");
            ViewData["ProjectId"] = new SelectList(ctx.Project, "Id", "CardId");
            ViewData["RequirementPriorityId"] = new SelectList(ctx.RequirementPriority, "Id", "Type");
            ViewBag.Type = new SelectList(new List<string> { "non_functional", "functional", "user", "business" });


            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View();
        }

        /// <summary>
        /// Posts the data to create a new project requirement.
        /// </summary>
        /// <param name="projectRequirement">The project requirement to create.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the create view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,RequirementPriorityId,ProjectId,Description")] ProjectRequirement projectRequirement, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Inserting a Project Requirement...");
            if (ModelState.IsValid)
            {
                projectRequirement.Id = Guid.NewGuid();
                ctx.Add(projectRequirement);
                try
                {
                    await ctx.SaveChangesAsync();
                } catch(Exception ex)
                {
                    TempData["ErrorMessage"] = "Unsuccessful add: An error occurred while saving the entity changes.";
                }
               
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(ctx.Project, "Id", "CardId", projectRequirement.ProjectId);
            ViewData["RequirementPriorityId"] = new SelectList(ctx.RequirementPriority, "Id", "Type", projectRequirement.RequirementPriorityId);

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            ViewBag.Type = new SelectList(new List<string> { "non_functional", "functional", "user", "business" });


            return View(projectRequirement);
        }

        /// <summary>
        /// Opens the form for editing an existing project requirement.
        /// </summary>
        /// <param name="id">The unique identifier of the project requirement to edit.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The edit view for a project requirement.</returns>
        public async Task<IActionResult> Edit(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Get a Project Requirement Edit page called...");
            if (id == null || ctx.ProjectRequirement == null)
            {
                return NotFound();
            }

            var projectRequirement = await ctx.ProjectRequirement.FindAsync(id);
            if (projectRequirement == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(ctx.Project, "Id", "CardId", projectRequirement.ProjectId);
            ViewData["RequirementPriorityId"] = new SelectList(ctx.RequirementPriority, "Id", "Type", projectRequirement.RequirementPriorityId);

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(projectRequirement);
        }

        /// <summary>
        /// Posts the updated data for an existing project requirement.
        /// </summary>
        /// <param name="id">The unique identifier of the project requirement being edited.</param>
        /// <param name="projectRequirement">The updated project requirement data.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the edit view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Type,RequirementPriorityId,ProjectId,Description")] ProjectRequirement projectRequirement, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Updating a Project Requirement...");
            if (id != projectRequirement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(projectRequirement);
                    await ctx.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectRequirementExists(projectRequirement.Id))
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
            ViewData["ProjectId"] = new SelectList(ctx.Project, "Id", "CardId", projectRequirement.ProjectId);
            ViewData["RequirementPriorityId"] = new SelectList(ctx.RequirementPriority, "Id", "Type", projectRequirement.RequirementPriorityId);

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(projectRequirement);
        }

        /// <summary>
        /// Retrieves a specific requirement task.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement task.</param>
        /// <returns>The partial view of the requirement task.</returns>
        [HttpGet]
        public async Task<IActionResult> GetRequirementTask(Guid id)
        {
            logger.LogInformation($"Getting task with id: {id}");
            var requirementTask = await ctx.RequirementTask
                                       .Where(o => o.Id == id)
                                       .Select(o => new RequirementTaskViewModel
                                       {
                                           Id = o.Id,
                                           PlannedStartDate = o.PlannedStartDate,
                                           PlannedEndDate = o.PlannedEndDate,
                                           ActualStartDate = o.ActualStartDate,
                                           ActualEndDate = o.ActualEndDate,
                                           TaskStatus = o.TaskStatus.ToString(),
                                           RequirementDescription = o.ProjectRequirement.Description,
                                           ProjectWork = o.ProjectWork.Description

                                       })
                                       .FirstOrDefaultAsync();

            if (requirementTask != null)
            {
                return PartialView(requirementTask);
            }
            else
            {
                return NotFound($"Unknown task id!: {id}");
            }
        }

        /// <summary>
        /// Opens the form for editing an existing requirement task.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement task to edit.</param>
        /// <returns>The edit view for a requirement task.</returns>
        [HttpGet]
        public async Task<IActionResult> EditRequirementTask(Guid id)
        {
            logger.LogInformation("Get Reqirement Task called...");
            var requirementTask = await ctx.RequirementTask
                                       .Where(o => o.Id == id)
                                       .Select(o => new RequirementTaskViewModel
                                       {
                                           Id = o.Id,
                                           PlannedStartDate = o.PlannedStartDate,
                                           PlannedEndDate = o.PlannedEndDate,
                                           ActualStartDate = o.ActualStartDate,
                                           ActualEndDate = o.ActualEndDate,
                                           TaskStatus = o.TaskStatus.ToString(),
                                           RequirementDescription = o.ProjectRequirement.Description,
                                           ProjectWork = o.ProjectWork.Description
                                       })
                                       .FirstOrDefaultAsync();

            if (requirementTask != null)
            {
                return PartialView(requirementTask);
            }
            else
            {
                return NotFound($"Unknown Task id: {id}");
            }
        }

        /// <summary>
        /// Posts the updated data for an existing requirement task.
        /// </summary>
        /// <param name="requirementTask">The updated requirement task data.</param>
        /// <returns>Redirects to the get requirement task view if successful, otherwise returns to the edit requirement task view.</returns>
        [HttpPost]
        public async Task<IActionResult> EditRequirementTask(RequirementTaskViewModel requirementTask)
        {
            logger.LogInformation("Inserting a Requirement Task...");
            if (requirementTask == null)
            {
                return NotFound("No data found");
            }
            RequirementTask dbRequirementTask = await ctx.RequirementTask.FindAsync(requirementTask.Id);
            if (dbRequirementTask == null)
            {
                return NotFound($"Unknown task id: {requirementTask.Id}");
            }

            logger.LogInformation($"Checking if the state is valid...");

            if (ModelState.IsValid)
            {

                logger.LogInformation($"State valid... Updating...");
                try
                {
                    dbRequirementTask.Id = requirementTask.Id;
                    dbRequirementTask.PlannedStartDate = requirementTask.PlannedStartDate;
                    dbRequirementTask.PlannedEndDate = requirementTask.PlannedEndDate;
                    dbRequirementTask.ActualStartDate = requirementTask.ActualStartDate;
                   
                  
                    await ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(GetRequirementTask), new { id = requirementTask.Id });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(requirementTask);
                }
            }
            else
            {
                logger.LogInformation($"State invalid...");
                return PartialView(requirementTask);
            }
        }

        /// <summary>
        /// Deletes a specific requirement task.
        /// </summary>
        /// <param name="Id">The unique identifier of the requirement task to delete.</param>
        /// <returns>An empty result if successful, otherwise redirects to the get requirement task view.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteRequirementTask(Guid Id)
        {
            logger.LogInformation("Deleting a Requirement Task...");
            ActionResponseMessage responseMessage;

            var requirementTask = ctx.RequirementTask.AsNoTracking().Where(o => o.Id == Id).SingleOrDefault();
            if (requirementTask != null)
            {
                try
                {
                    ctx.Remove(requirementTask);
                    await ctx.SaveChangesAsync();
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Task successfully deleted!");
                }
                catch (Exception exc)
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Error during task deletion: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Task with id: '{Id}' does not exist!");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return responseMessage.MessageType == MessageType.Success ?
             new EmptyResult() : await GetRequirementTask(Id);

        }



        /// <summary>
        /// Opens the form for deleting an existing project requirement.
        /// </summary>
        /// <param name="id">The unique identifier of the project requirement to delete.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>The delete view for a project requirement.</returns>
        public async Task<IActionResult> Delete(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Get a Project Requirement Delete page called...");
            if (id == null || ctx.ProjectRequirement == null)
            {
                return NotFound();
            }

            var projectRequirement = await ctx.ProjectRequirement
                .Include(p => p.Project)
                .Include(p => p.RequirementPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectRequirement == null)
            {
                return NotFound();
            }

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(projectRequirement);
        }

        /// <summary>
        /// Confirms the deletion of a specific project requirement.
        /// </summary>
        /// <param name="id">The unique identifier of the project requirement to delete.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the delete view.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
            logger.LogInformation("Deleting a Project Requirement...");
            if (ctx.ProjectRequirement == null)
            {
                return Problem("Entity set 'Rppp01Context.ProjectRequirement'  is null.");
            }
            var projectRequirement = await ctx.ProjectRequirement.FindAsync(id);
            if (projectRequirement != null)
            {
                           ctx.ProjectRequirement.Remove(projectRequirement);
            }
            
            try
            {
                await ctx.SaveChangesAsync();
            } catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unsuccessful deletion: An error occurred while deleting the entity.";
                return View(projectRequirement);
            }
           

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Opens the form for creating a new requirement task.
        /// </summary>
        /// <param name="projectRequirementId">The unique identifier of the project requirement for the new task.</param>
        /// <returns>The create view for a requirement task.</returns>
        [HttpGet]
        public async Task<IActionResult> NewRequirementTask(Guid projectRequirementId)
        { 
            logger.LogInformation("NewRequirementTask GET");
            await PrepareDropDownLists(projectRequirementId);

            ViewBag.projectRequirementId = projectRequirementId;
            logger.LogInformation($"ProjectRequirementId: {projectRequirementId}");
            return PartialView();
        }

        /// <summary>
        /// Posts the data to create a new requirement task.
        /// </summary>
        /// <param name="requirementTask">The requirement task to create.</param>
        /// <returns>Content indicating success or failure, or the partial view for creating a new requirement task.</returns>
        [HttpPost]
        public async Task<IActionResult> NewRequirementTask(RequirementTask requirementTask)
        {
            ActionResponseMessage responseMessage;
            logger.LogInformation("NewRequirementTask POST");
            logger.LogInformation("Checking if model is valid...");

            if (ModelState.IsValid)
            {
                logger.LogInformation("Model is valid!");
                try
                {
                    // Generate a new GUID for the RequirementTask
                    requirementTask.Id = Guid.NewGuid();

                    var existingProjectWork = await ctx.ProjectWork
                            .AsNoTracking()
                            .FirstOrDefaultAsync(pw => pw.Id == requirementTask.ProjectWork.Id);

                    if (requirementTask.ProjectWork != null && requirementTask.ProjectWork.Id != Guid.Empty)
                    {
                        logger.LogInformation("ProjectWork is not null and has an Id");

                        // Retrieve existing ProjectWork from the context or database
                        

                        if (existingProjectWork == null)
                        {
                            throw new InvalidOperationException("Specified ProjectWork does not exist.");
                        }

                        // Attach the existing ProjectWork to the context
                        ctx.Attach(existingProjectWork);
                        requirementTask.ProjectWork = existingProjectWork;
                    }

                    logger.LogInformation($"Adding new task: '{requirementTask.ProjectWork?.Id}', '{requirementTask.ProjectWork?.Title}', '{requirementTask.ProjectRequirementId}'");
                    ctx.Add(requirementTask);
                    requirementTask.Id = existingProjectWork.Id;
                    await ctx.SaveChangesAsync();

                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Task with id: '{requirementTask.Id}' successfully added!.");
                }
                catch (Exception exc)
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Error during task creation: {exc.Message}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    await PrepareDropDownLists(requirementTask.ProjectRequirement.Id);
                }
            }
            else
            {
                logger.LogInformation("Model is not valid!");
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Error during task creation!");

                await PrepareDropDownLists(requirementTask.ProjectRequirement.Id);
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return responseMessage.MessageType == MessageType.Success ?
            Content($"<script>setTimeout(function() {{ window.location.href='/rppp/01/ProjectRequirements/Details/{requirementTask.ProjectRequirementId}?page=1&sort=1&ascending=True'; }}, 1000);</script>", "text/html") : PartialView(requirementTask);
        }

        /// <summary>
        /// Checks if a project requirement exists in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the project requirement.</param>
        /// <returns>True if it exists, otherwise false.</returns>
        private bool ProjectRequirementExists(Guid id)
        {
          return ctx.ProjectRequirement.Any(e => e.Id == id);
        }

        /// <summary>
        /// Prepares dropdown lists for the requirement task view.
        /// </summary>
        /// <param name="projectRequirementId">The unique identifier of the project requirement for which the task is being created or edited.</param>
        /// <returns>Task.</returns>
        private async Task PrepareDropDownLists(Guid projectRequirementId)
        {
            logger.LogInformation($"Preparing drop down lists for project requirement id: {projectRequirementId}");
            var currentProjectId = await ctx.ProjectRequirement
                .Where(pr => pr.Id == projectRequirementId)
                .Select(pr => pr.ProjectId)
                .FirstOrDefaultAsync();

            logger.LogInformation($"Preparing drop down lists for project id: {currentProjectId}");
            var taskStatuses = await ctx.TaskStatus
                                  .ToListAsync();

            var projectWorks = await ctx.ProjectWork
                                .Where(pw => pw.ProjectId == currentProjectId)
                                .ToListAsync();

            logger.LogInformation($"ProjectWorks: {projectWorks.Count}");

            var requirementTaskIds = await ctx.RequirementTask
                            .Where(rt => rt.ProjectRequirement.ProjectId == currentProjectId)                    
                            .Select(rt => rt.Id).ToListAsync();

            logger.LogInformation($"RequirementTaskIds: {requirementTaskIds.Count}");

            var availableProjectWorks = projectWorks.Where(pw => !requirementTaskIds.Contains(pw.Id)).ToList();

            logger.LogInformation($"AvailableProjectWorks: {availableProjectWorks.Count}");
            var statusesList = taskStatuses.Select(status => new SelectListItem
            {
                Text = $"{status.Type}",
                Value = status.Id.ToString()
            }).ToList();

            var projectWorksList = availableProjectWorks.Select(projectWork => new SelectListItem
            {
                Text = $"{projectWork.Title}",
                Value = projectWork.Id.ToString()
            }).ToList();


            ViewBag.ProjectWorks = projectWorksList;
            ViewBag.TaskStatuses = statusesList;
        }


    }


}
