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
    public class RequirementTasksController : Controller
    {
        private readonly Rppp01Context _context;

        public RequirementTasksController(Rppp01Context context)
        {
            _context = context;
        }

        // GET: RequirementTasks
        public async Task<IActionResult> Index()
        {
            var rppp01Context = _context.RequirementTask.Include(r => r.ProjectRequirement).Include(r => r.ProjectWork).Include(r => r.TaskStatus);
            return View(await rppp01Context.ToListAsync());
        }

        // GET: RequirementTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.RequirementTask == null)
            {
                return NotFound();
            }

            var requirementTask = await _context.RequirementTask
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
            ViewData["ProjectRequirementId"] = new SelectList(_context.ProjectRequirement, "Id", "Description");
            ViewData["Id"] = new SelectList(_context.ProjectWork, "Id", "Title");
            ViewData["TaskStatusId"] = new SelectList(_context.TaskStatus, "Id", "Type");
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
                requirementTask.Id = Guid.NewGuid();
                _context.Add(requirementTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectRequirementId"] = new SelectList(_context.ProjectRequirement, "Id", "Description", requirementTask.ProjectRequirementId);
            ViewData["Id"] = new SelectList(_context.ProjectWork, "Id", "Title", requirementTask.Id);
            ViewData["TaskStatusId"] = new SelectList(_context.TaskStatus, "Id", "Type", requirementTask.TaskStatusId);
            return View(requirementTask);
        }

        // GET: RequirementTasks/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.RequirementTask == null)
            {
                return NotFound();
            }

            var requirementTask = await _context.RequirementTask.FindAsync(id);
            if (requirementTask == null)
            {
                return NotFound();
            }
            ViewData["ProjectRequirementId"] = new SelectList(_context.ProjectRequirement, "Id", "Description", requirementTask.ProjectRequirementId);
            ViewData["Id"] = new SelectList(_context.ProjectWork, "Id", "Title", requirementTask.Id);
            ViewData["TaskStatusId"] = new SelectList(_context.TaskStatus, "Id", "Type", requirementTask.TaskStatusId);
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
                    _context.Update(requirementTask);
                    await _context.SaveChangesAsync();
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
            ViewData["ProjectRequirementId"] = new SelectList(_context.ProjectRequirement, "Id", "Description", requirementTask.ProjectRequirementId);
            ViewData["Id"] = new SelectList(_context.ProjectWork, "Id", "Title", requirementTask.Id);
            ViewData["TaskStatusId"] = new SelectList(_context.TaskStatus, "Id", "Type", requirementTask.TaskStatusId);
            return View(requirementTask);
        }

        // GET: RequirementTasks/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.RequirementTask == null)
            {
                return NotFound();
            }

            var requirementTask = await _context.RequirementTask
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
            if (_context.RequirementTask == null)
            {
                return Problem("Entity set 'Rppp01Context.RequirementTask'  is null.");
            }
            var requirementTask = await _context.RequirementTask.FindAsync(id);
            if (requirementTask != null)
            {
                _context.RequirementTask.Remove(requirementTask);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RequirementTaskExists(Guid id)
        {
          return _context.RequirementTask.Any(e => e.Id == id);
        }
    }
}
