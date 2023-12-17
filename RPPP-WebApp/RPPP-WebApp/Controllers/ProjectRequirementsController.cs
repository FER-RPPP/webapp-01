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
                await ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(ctx.Project, "Id", "CardId", projectRequirement.ProjectId);
            ViewData["RequirementPriorityId"] = new SelectList(ctx.RequirementPriority, "Id", "Type", projectRequirement.RequirementPriorityId);

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

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
            
            await ctx.SaveChangesAsync();

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectRequirementExists(Guid id)
        {
          return ctx.ProjectRequirement.Any(e => e.Id == id);
        }
    }
}
