using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Text.Json;

namespace RPPP_WebApp.Controllers
{
    public class OrganizationController : Controller
    {
        private readonly Rppp01Context ctx;
        private readonly ILogger<TransactionTypeController> logger;
        private readonly AppSettings appData;
        public OrganizationController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<TransactionTypeController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            var query = ctx.Organization.AsNoTracking();
                          
            int pagesize = appData.PageSize;
            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji niti jedna organizacija.");
                TempData[Constants.Message] = "Ne postoji niti jedna organizacija.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Create));
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

            var organizations = query
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .ToList();

            var model = new OrganizationViewModel
            {
                Organizations = organizations,
                PagingInfo = pagingInfo
            };

            return View(model);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Organization organization)
        {
            logger.LogTrace(JsonSerializer.Serialize(organization));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(organization);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Organizacija {organization.Name} je dodana.");

                    TempData[Constants.Message] = $"Organizacija {organization.Name} je dodana";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove organizacije: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(organization);
                }
            }
            else
            {
                return View(organization);
            }
        }
        [HttpGet]
        public IActionResult Edit(Guid Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var organization = ctx.Organization.AsNoTracking().Where(o => o.Id == Id).SingleOrDefault();
            if (organization == null)
            {
                logger.LogWarning("Ne postoji ta organizacija");
                return NotFound("Ne postoji ta organizacija");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(organization);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid Id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Organization organization = await ctx.Organization
                                  .Where(o => o.Id == Id)
                                  .FirstOrDefaultAsync();
                if (organization == null)
                {
                    return NotFound("Neispravan id organizacije");
                }

                if (await TryUpdateModelAsync<Organization>(organization, "",
                    o => o.Name, o => o.Id
                ))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = $"Organizacija {organization.Name} ažurirana.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(organization);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o organizaciji nije moguće povezati s forme");
                    return View(organization);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), Id);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var organization = ctx.Organization.Find(Id);
            if (organization != null)
            {
                try
                {
                    ctx.Remove(organization);
                    ctx.SaveChanges();
                    logger.LogInformation($"Organizacija {organization.Name} uspješno obrisana.");
                    TempData[Constants.Message] = $"Organizacija {organization.Name} uspješno obrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja organizacije: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja organizacije: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji organizacija");
                TempData[Constants.Message] = "Ne postoji organizacija";
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }
    }
}

