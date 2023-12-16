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
    public class RequirementPrioritiesController : Controller
    {
        private readonly Rppp01Context _context;

        public RequirementPrioritiesController(Rppp01Context context)
        {
            _context = context;
        }

        // GET: RequirementPriorities
        public async Task<IActionResult> Index()
        {
              return View(await _context.RequirementPriority.ToListAsync());
        }

        // GET: RequirementPriorities/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.RequirementPriority == null)
            {
                return NotFound();
            }

            var requirementPriority = await _context.RequirementPriority
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requirementPriority == null)
            {
                return NotFound();
            }

            return View(requirementPriority);
        }

        // GET: RequirementPriorities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RequirementPriorities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type")] RequirementPriority requirementPriority)
        {
            if (ModelState.IsValid)
            {
                requirementPriority.Id = Guid.NewGuid();
                _context.Add(requirementPriority);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(requirementPriority);
        }

        // GET: RequirementPriorities/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.RequirementPriority == null)
            {
                return NotFound();
            }

            var requirementPriority = await _context.RequirementPriority.FindAsync(id);
            if (requirementPriority == null)
            {
                return NotFound();
            }
            return View(requirementPriority);
        }

        // POST: RequirementPriorities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Type")] RequirementPriority requirementPriority)
        {
            if (id != requirementPriority.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(requirementPriority);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequirementPriorityExists(requirementPriority.Id))
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
            return View(requirementPriority);
        }

        // GET: RequirementPriorities/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.RequirementPriority == null)
            {
                return NotFound();
            }

            var requirementPriority = await _context.RequirementPriority
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requirementPriority == null)
            {
                return NotFound();
            }

            return View(requirementPriority);
        }

        // POST: RequirementPriorities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.RequirementPriority == null)
            {
                return Problem("Entity set 'Rppp01Context.RequirementPriority'  is null.");
            }
            var requirementPriority = await _context.RequirementPriority.FindAsync(id);
            if (requirementPriority != null)
            {
                _context.RequirementPriority.Remove(requirementPriority);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RequirementPriorityExists(Guid id)
        {
          return _context.RequirementPriority.Any(e => e.Id == id);
        }
    }
}
