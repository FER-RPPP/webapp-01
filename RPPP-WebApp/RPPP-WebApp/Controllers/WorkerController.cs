using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Text.Json;

namespace RPPP_WebApp.Controllers
{
    public class WorkerController : Controller
    {
        private readonly Rppp01Context ctx;
        private readonly ILogger<TransactionTypeController> logger;
        private readonly AppSettings appData;
        public WorkerController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<TransactionTypeController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        public async Task<IActionResult> Index(WorkerFilter filter, int page = 1, int sort = 1, bool ascending = true)
        {
            var query = ctx.Worker
                           .AsNoTracking();
            int pagesize = appData.PageSize;

            string errorMessage = "Ne postoji niti jedan radnik";

            if (!string.IsNullOrEmpty(filter.Organization))
            {
                query = query.Where(p => p.Organization.Name.Contains(filter.Organization));
                errorMessage += $" u organizaciji: {filter.Organization}";
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

            var workers = await query
            .Select(o => new WorkerViewModel
            {
               Id = o.Id,
               Email = o.Email,
               FirstName = o.FirstName,
               LastName = o.LastName,
               PhoneNumber = o.PhoneNumber,
               Organization = o.Organization.Name
              
            })
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .ToListAsync();

            var model = new WorkerViewsModel
            {
                Workers = workers,
                PagingInfo = pagingInfo,
                Filter = filter
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Worker worker)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(worker);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Radnik je dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownLists();
                    return View(worker);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return View(worker);
            }
        }

        public async Task<IActionResult> Edit(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
            var worker = ctx.Worker.AsNoTracking().Where(o => o.Id == id).SingleOrDefault();
            if (worker == null)
            {
                logger.LogWarning("Ne postoji radnik: " + id);
                return NotFound("Ne postoji radnik: " + id);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownLists();
                return View(worker);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Worker worker = await ctx.Worker
                                  .Where(o => o.Id == id)
                                  .FirstOrDefaultAsync();
                if (worker == null)
                {
                    return NotFound("Neispravan id radnika: " + id);
                }

                if (await TryUpdateModelAsync(worker, "",
                    o => o.Id, o => o.Email, o => o.FirstName, o => o.LastName, o => o.PhoneNumber, o => o.OrganizationId
                ))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = $"Radnik je ažuriran.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        await PrepareDropDownLists();
                        return View(worker);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o radniku nije moguće povezati s forme");
                    await PrepareDropDownLists();
                    return View(worker);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), new { id = id, page = page, sort = sort, ascending = ascending });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var worker = ctx.Worker.Find(Id);
            if (worker != null)
            {
                try
                {
                    ctx.Remove(worker);
                    ctx.SaveChanges();
                    logger.LogInformation($"Radnik uspješno obrisan.");
                    TempData[Constants.Message] = $"Radnik uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja radnika: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja radnika: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji radnik: ", Id);
                TempData[Constants.Message] = "Ne postoji radnik: " + Id;
                TempData[Constants.ErrorOccurred] = true;
            }

            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }

        private async Task PrepareDropDownLists()
        {
            var organizations = await ctx.Organization
                                 .ToListAsync();


            var organizationList = organizations.Select(organization => new SelectListItem
            {
                Text = $"{organization.Name}",
                Value = organization.Id.ToString()
            }).ToList();


            ViewBag.Organizations = organizationList;
        }
    }
}
