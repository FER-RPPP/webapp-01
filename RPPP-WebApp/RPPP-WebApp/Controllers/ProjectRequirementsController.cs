using System;
using System.Collections.Generic;
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

        // GET: ProjectRequirements
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
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

        // GET: ProjectRequirements/Details/5
        public async Task<IActionResult> Details(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
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

        // GET: ProjectRequirements/Create
        public IActionResult Create(int page = 1, int sort = 1, bool ascending = true)
        {
            ViewData["ProjectId"] = new SelectList(ctx.Project, "Id", "CardId");
            ViewData["RequirementPriorityId"] = new SelectList(ctx.RequirementPriority, "Id", "Type");
            ViewBag.Type = new SelectList(new List<string> { "non_functional", "functional", "user", "business" });


            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View();
        }

        // POST: ProjectRequirements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,RequirementPriorityId,ProjectId,Description")] ProjectRequirement projectRequirement, int page = 1, int sort = 1, bool ascending = true)
        {
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

        // GET: ProjectRequirements/Edit/5
        public async Task<IActionResult> Edit(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
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

        // POST: ProjectRequirements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Type,RequirementPriorityId,ProjectId,Description")] ProjectRequirement projectRequirement, int page = 1, int sort = 1, bool ascending = true)
        {
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

        [HttpGet]
        public async Task<IActionResult> EditRequirementTask(Guid id)
        {
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

            await PrepareDropDownLists();
            if (requirementTask != null)
            {
                return PartialView(requirementTask);
            }
            else
            {
                return NotFound($"Unknown Task id: {id}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditRequirementTask(RequirementTaskViewModel requirementTask)
        {
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

        [HttpDelete]
        public async Task<IActionResult> DeleteRequirementTask(Guid Id)
        {
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



        // GET: ProjectRequirements/Delete/5
        public async Task<IActionResult> Delete(Guid? id, int page = 1, int sort = 1, bool ascending = true)
        {
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

        // POST: ProjectRequirements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
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

        private bool ProjectRequirementExists(Guid id)
        {
          return ctx.ProjectRequirement.Any(e => e.Id == id);
        }

        private async Task PrepareDropDownLists()
        {
            var owners = await ctx.Owner
                                  .ToListAsync();

            var types = await ctx.TransactionType
                                .ToListAsync();

            var purposes = await ctx.TransactionPurpose
                                .ToListAsync();

            var ownersList = owners.Select(owner => new SelectListItem
            {
                Text = $"{owner.Name} {owner.Surname} ({owner.Oib})",
                Value = owner.Oib.ToString()
            }).ToList();

            var typeList = types.Select(type => new SelectListItem
            {
                Text = $"{type.TypeName}",
                Value = type.Id.ToString()
            }).ToList();

            var purposeList = purposes.Select(purpose => new SelectListItem
            {
                Text = $"{purpose.PurposeName}",
                Value = purpose.Id.ToString()
            }).ToList();

            ViewBag.Types = typeList;
            ViewBag.Purposes = purposeList;
            ViewBag.Owners = ownersList;
        }
    }

    
}
