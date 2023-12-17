using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.Views;

namespace RPPP_WebApp.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly Rppp01Context _context;

        public ProjectsController(Rppp01Context context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index(string sortOrder, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["TypeSortParm"] = sortOrder == "Type" ? "type_desc" : "Type";
            ViewData["CurrentFilter"] = searchString;

            var projects = from p in _context.Project
                           .Include(p => p.Card)
                           .Include(p => p.Client)
                           .Include(p => p.Owner)
                           select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                projects = projects.Where(p => p.Name.Contains(searchString)
                                       || p.Type.Contains(searchString)
                                       || p.Name.Contains(searchString)
                                       || p.Owner.Name.Contains(searchString)
                                       || p.Owner.Surname.Contains(searchString)
                                       || p.Owner.Oib.Contains(searchString)
                                       || p.Client.LastName.Contains(searchString)
                                       || p.Client.FirstName.Contains(searchString)
                                       || p.Client.Oib.Contains(searchString)
                                       || p.Card.Iban.Contains(searchString));

            }

            switch (sortOrder)
            {
                case "name_desc":
                    projects = projects.OrderByDescending(p => p.Name);
                    break;
                case "name_asc":
                    projects = projects.OrderBy(p => p.Name);
                    break;
                case "type_desc":
                    projects = projects.OrderByDescending(p => p.Type);
                    break;
                case "type_asc":
                    projects = projects.OrderBy(p => p.Type);
                    break;
                case "iban_desc":
                    projects = projects.OrderByDescending(p => p.Card.Iban);
                    break;
                case "iban_asc":
                    projects = projects.OrderBy(p => p.Card.Iban);
                    break;
                case "client_desc":
                    projects = projects.OrderByDescending(p => p.Client.LastName);
                    break;
                case "client_asc":
                    projects = projects.OrderBy(p => p.Client.LastName);
                    break;
                case "owner_desc":
                    projects = projects.OrderByDescending(p => p.Owner.Surname); 
                    break;
                case "owner_asc":
                    projects = projects.OrderBy(p => p.Owner.Surname);
                    break;
                default:
                    projects = projects.OrderBy(p => p.Name);
                    break;
            }


            int pageSize = 8; 
            return View(await PaginatedList<Project>.CreateAsync(projects.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Card)
                .Include(p => p.Client)
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            ViewData["Card"] = _context.ProjectCard
                .Select(c => new SelectListItem
                {
                    Value = c.Iban.ToString(),
                    Text = $"{c.Iban} ({c.Balance} €)"
                })
                .ToList();

            ViewData["Client"] = _context.Client
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName} ({c.Oib})"
                })
                .ToList();

            ViewData["Owner"] = _context.Owner
                .Select(o => new SelectListItem
                {
                    Value = o.Oib.ToString(),
                    Text = $"{o.Name} {o.Surname} ({o.Oib})"
                })
                .ToList();

            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,Name,Type,OwnerId,CardId")] Project project)
        {

            if (!ModelState.IsValid)
            {
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }

            try
            {
                project.Id = Guid.NewGuid();
                _context.Add(project);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Project has been successfully created.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbException ex)
            {
                string errorMessage = "A database update error occurred.";

                if (ex.InnerException is SqlException sqlEx)
                {
                    switch (sqlEx.Number)
                    {
                        case 547: // FK constraint
                            errorMessage = "Operation failed due to a relational constraint. Please ensure all related data is valid.";
                            break;
                        case 2601: // Duplicated key
                        case 2627:
                            errorMessage = "This record already exists. Please ensure the data is unique.";
                            break;
                    }
                }

                TempData["ErrorMessage"] = errorMessage;
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);

            // Assuming Client has properties FirstName, LastName, and Id
            ViewData["ClientId"] = new SelectList(_context.Client.Select(c => new {
                Id = c.Id,
                FullName = $"{c.FirstName} {c.LastName} ({c.Oib})"
            }), "Id", "FullName", project.ClientId);

            // Assuming Owner has properties Name, Surname, and Oib
            ViewData["OwnerId"] = new SelectList(_context.Owner.Select(o => new {
                Oib = o.Oib,
                FullName = $"{o.Name} {o.Surname} ({o.Oib})"
            }), "Oib", "FullName", project.OwnerId);

            return View(project);
        }


        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ClientId,Name,Type,OwnerId,CardId")] Project project)
        {

            if (!ModelState.IsValid)
            {
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }

            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Project has been successfully updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                string errorMessage = "A database update error occurred.";

                if (ex.InnerException is SqlException sqlEx)
                {
                    switch (sqlEx.Number)
                    {
                        case 547: // FK constraint
                            errorMessage = "Operation failed due to a relational constraint. Please ensure all related data is valid.";
                            break;
                        case 2601: // Duplicated key
                        case 2627:
                            errorMessage = "This record already exists. Please ensure the data is unique.";
                            break;
                    }
                }

                TempData["ErrorMessage"] = errorMessage;
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                ViewData["CardId"] = new SelectList(_context.ProjectCard, "Iban", "Iban", project.CardId);
                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Email", project.ClientId);
                ViewData["OwnerId"] = new SelectList(_context.Owner, "Oib", "Oib", project.OwnerId);
                return View(project);
            }
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Card)
                .Include(p => p.Client)
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Project == null)
            {
                return Problem("Entity set 'Rppp01Context.Project'  is null.");
            }
            var project = await _context.Project.FindAsync(id);

            if (!ModelState.IsValid)
            {
                return View(project);
            }

            try
            {
                if (project != null)
                {
                    _context.Project.Remove(project);
                }
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Project has been successfully deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                string errorMessage = "A database update error occurred.";

                if (ex.InnerException is SqlException sqlEx)
                {
                    switch (sqlEx.Number)
                    {
                        case 547: // FK constraint
                            errorMessage = "Operation failed due to a relational constraint. Please ensure all related data is valid.";
                            break;
                        case 2601: // Duplicated key
                        case 2627:
                            errorMessage = "This record already exists. Please ensure the data is unique.";
                            break;
                    }
                }

                TempData["ErrorMessage"] = errorMessage;
                return View(project);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(project);
            }
        }
        

    }
}
