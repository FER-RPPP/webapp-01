using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.Views;

namespace RPPP_WebApp.Controllers
{
    public class ClientsController : Controller
    {
        private readonly Rppp01Context _context;

        public ClientsController(Rppp01Context context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index(string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["OibSortParm"] = sortOrder == "Oib" ? "oib_desc" : "Oib";
            ViewData["IbanSortParm"] = sortOrder == "Iban" ? "iban_desc" : "Iban";
            ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";
            ViewData["FirstNameSortParm"] = sortOrder == "FirstName" ? "first_name_desc" : "FirstName";
            ViewData["LastNameSortParm"] = sortOrder == "LastName" ? "last_name_desc" : "LastName";

            var clients = from c in _context.Client select c;


            switch (sortOrder)
            {
                case "oib_desc":
                    clients = clients.OrderByDescending(c => c.Oib);
                    break;
                case "Oib":
                    clients = clients.OrderBy(c => c.Oib);
                    break;
                case "iban_desc":
                    clients = clients.OrderByDescending(c => c.Iban);
                    break;
                case "Iban":
                    clients = clients.OrderBy(c => c.Iban);
                    break;
                case "email_desc":
                    clients = clients.OrderByDescending(c => c.Email);
                    break;
                case "Email":
                    clients = clients.OrderBy(c => c.Email);
                    break;
                case "first_name_desc":
                    clients = clients.OrderByDescending(c => c.FirstName);
                    break;
                case "FirstName":
                    clients = clients.OrderBy(c => c.FirstName);
                    break;
                case "last_name_desc":
                    clients = clients.OrderByDescending(c => c.LastName);
                    break;
                case "LastName":
                    clients = clients.OrderBy(c => c.LastName);
                    break;
                default:
                    clients = clients.OrderBy(c => c.LastName); // Default sorting
                    break;
            }

            int pageSize = 8; // Adjust as needed
            return View(await PaginatedList<Client>.CreateAsync(clients.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        // GET: Clients/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Client == null)
            {
                return NotFound();
            }

            var client = await _context.Client
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Oib,Iban,Email,FirstName,LastName")] Client client)
        {
            if (!ModelState.IsValid)
            {
                return View(client);
            }

            try
            {
                client.Id = Guid.NewGuid();
                _context.Add(client);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Client has been successfully added.";
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
                return View(client);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(client);
            }

        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Client == null)
            {
                return NotFound();
            }

            var client = await _context.Client.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Oib,Iban,Email,FirstName,LastName")] Client client)
        {

            if (!ModelState.IsValid)
            {
                return View(client);
            }

            try
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Client has been successfully updated.";
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
                return View(client);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(client);
            }

        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Client == null)
            {
                return NotFound();
            }

            var client = await _context.Client
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);

        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Client == null)
            {
                TempData["StatusMessage"] = "Entity set 'Rppp01Context.Client'  is null.";
                return Problem(nameof(Client));
            }
            var client = await _context.Client.FindAsync(id);


            if (!ModelState.IsValid)
            {
                return View(client);
            }

            try
            {
                if (client != null)
                {
                    _context.Client.Remove(client);
                }
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Client has been successfully deleted.";
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
                return View(client);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(client);
            }
        }

    }
}
