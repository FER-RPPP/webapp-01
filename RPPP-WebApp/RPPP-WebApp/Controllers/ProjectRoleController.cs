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
    public class ProjectRoleController : Controller
    {
        private readonly Rppp01Context ctx;
        private readonly ILogger<TransactionTypeController> logger;
        private readonly AppSettings appData;
        public ProjectRoleController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<TransactionTypeController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            var query = ctx.ProjectRole.AsNoTracking();

            int pagesize = appData.PageSize;
            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji niti jedna uloga.");
                TempData[Constants.Message] = "Ne postoji niti jedna uloga.";
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

            var roles = query
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .ToList();

            var model = new ProjectRoleViewModel
            {
                Roles = roles,
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
        public IActionResult Create(ProjectRole projectRole)
        {
            logger.LogTrace(JsonSerializer.Serialize(projectRole));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(projectRole);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Uloga {projectRole.Name} je dodana.");

                    TempData[Constants.Message] = $"Uloga {projectRole.Name} je dodana";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove uloge: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(projectRole);
                }
            }
            else
            {
                return View(projectRole);
            }
        }
        [HttpGet]
        public IActionResult Edit(Guid Id, int page = 1, int sort = 1, bool ascending = true)
        {
            var projectRole = ctx.ProjectRole.AsNoTracking().Where(o => o.Id == Id).SingleOrDefault();
            if (projectRole == null)
            {
                logger.LogWarning("Ne postoji ta uloga");
                return NotFound("Ne postoji ta uloga");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(projectRole);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid Id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                ProjectRole projectRole = await ctx.ProjectRole
                                  .Where(o => o.Id == Id)
                                  .FirstOrDefaultAsync();
                if (projectRole == null)
                {
                    return NotFound("Neispravan id uloge");
                }

                if (await TryUpdateModelAsync<ProjectRole>(projectRole, "",
                    o => o.Name, o => o.Id
                ))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = $"Uloga {projectRole.Name} ažurirana.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(projectRole);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o ulozi nije moguće povezati s forme");
                    return View(projectRole);
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
            var projectRole = ctx.ProjectRole.Find(Id);
            if (projectRole != null)
            {
                try
                {
                    ctx.Remove(projectRole);
                    ctx.SaveChanges();
                    logger.LogInformation($"Uloga {projectRole.Name} uspješno obrisana.");
                    TempData[Constants.Message] = $"Uloga {projectRole.Name} uspješno obrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja uloge: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja uloge: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji uloga");
                TempData[Constants.Message] = "Ne postoji uloga";
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }
    }
}
