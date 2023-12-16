using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers
{
    public class ProjectRequirementsController : Controller
    {
        private readonly Rppp01Context _context;

        public ProjectRequirementsController(Rppp01Context context)
        {
            _context = context;
        }

        // GET: ProjectRequirements
        public async Task<IActionResult> Index()
        {
            var rppp01Context = _context.ProjectRequirement.Include(p => p.Project).Include(p => p.RequirementPriority);
            return View(await rppp01Context.ToListAsync());
        }

        // GET: ProjectRequirements/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.ProjectRequirement == null)
            {
                return NotFound();
            }

            var projectRequirement = await _context.ProjectRequirement
                .Include(p => p.Project)
                .Include(p => p.RequirementPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectRequirement == null)
            {
                return NotFound();
            }

            return View(projectRequirement);
        }

        // GET: ProjectRequirements/Create
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "CardId");
            ViewData["RequirementPriorityId"] = new SelectList(_context.RequirementPriority, "Id", "Type");
            return View();
        }

        // POST: ProjectRequirements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,RequirementPriorityId,ProjectId,Description")] ProjectRequirement projectRequirement)
        {
            if (ModelState.IsValid)
            {
                projectRequirement.Id = Guid.NewGuid();
                _context.Add(projectRequirement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "CardId", projectRequirement.ProjectId);
            ViewData["RequirementPriorityId"] = new SelectList(_context.RequirementPriority, "Id", "Type", projectRequirement.RequirementPriorityId);
            return View(projectRequirement);
        }

        // GET: ProjectRequirements/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.ProjectRequirement == null)
            {
                return NotFound();
            }

            var projectRequirement = await _context.ProjectRequirement.FindAsync(id);
            if (projectRequirement == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "CardId", projectRequirement.ProjectId);
            ViewData["RequirementPriorityId"] = new SelectList(_context.RequirementPriority, "Id", "Type", projectRequirement.RequirementPriorityId);
            return View(projectRequirement);
        }

        // POST: ProjectRequirements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Type,RequirementPriorityId,ProjectId,Description")] ProjectRequirement projectRequirement)
        {
            if (id != projectRequirement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projectRequirement);
                    await _context.SaveChangesAsync();
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
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "CardId", projectRequirement.ProjectId);
            ViewData["RequirementPriorityId"] = new SelectList(_context.RequirementPriority, "Id", "Type", projectRequirement.RequirementPriorityId);
            return View(projectRequirement);
        }

        // GET: ProjectRequirements/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.ProjectRequirement == null)
            {
                return NotFound();
            }

            var projectRequirement = await _context.ProjectRequirement
                .Include(p => p.Project)
                .Include(p => p.RequirementPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectRequirement == null)
            {
                return NotFound();
            }

            return View(projectRequirement);
        }

        // POST: ProjectRequirements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.ProjectRequirement == null)
            {
                return Problem("Entity set 'Rppp01Context.ProjectRequirement'  is null.");
            }
            var projectRequirement = await _context.ProjectRequirement.FindAsync(id);
            if (projectRequirement != null)
            {
                _context.ProjectRequirement.Remove(projectRequirement);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectRequirementExists(Guid id)
        {
          return _context.ProjectRequirement.Any(e => e.Id == id);
        }
    }
}
