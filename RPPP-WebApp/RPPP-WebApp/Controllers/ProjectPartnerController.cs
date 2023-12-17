using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog.Filters;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    public class ProjectPartnerController : Controller
    {

        private readonly Rppp01Context ctx;
        private readonly ILogger<TransactionTypeController> logger;
        private readonly AppSettings appData;
        public ProjectPartnerController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<TransactionTypeController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        public async Task <IActionResult> Index(ProjectPartnerFilter filter, int page = 1, int sort = 1, bool ascending = true)
        {
            var query = ctx.ProjectPartner
                           .AsNoTracking();
            int pagesize = appData.PageSize;

            string errorMessage = "Ne postoji niti jedan zapis";

            if (!string.IsNullOrEmpty(filter.Project))
            {
                query = query.Where(p => p.Project.Name.Contains(filter.Project));
                errorMessage += $" gdje je projekt: {filter.Project}";
            }
            if (!string.IsNullOrEmpty(filter.Worker))
            {
                query = query.Where(p => p.Worker.Email.Contains(filter.Worker));
                errorMessage += $" gdje je radnik: {filter.Worker}";
            }
            if (!string.IsNullOrEmpty(filter.Role))
            {
                query = query.Where(p => p.Role.Name.Contains(filter.Role));
                errorMessage += $" gdje je uloga: {filter.Role}";
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

            var partners = await query
            .Select(o => new ProjectPartnerViewModel
            {
                Id = o.Id,
                Project = o.Project.Name,
                Worker = $"{o.Worker.FirstName} {o.Worker.LastName}",
                Role = o.Role.Name,
                DateFrom = o.DateFrom,
                DateTo = o.DateTo
            })
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .ToListAsync();

            var model = new ProjectPartnersViewModel
            {
                Partners = partners,
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
        public async Task<IActionResult> Create(ProjectPartner partner)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    partner.Id = Guid.NewGuid();
                    ctx.Add(partner);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Suradnik je dodan na projekt.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownLists();
                    return View(partner);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return View(partner);
            }
        }
        public async Task<IActionResult> Edit(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
            var partner = ctx.ProjectPartner.AsNoTracking().Where(o => o.Id == id).SingleOrDefault();
            if (partner  == null)
            {
                logger.LogWarning("Ne postoji suradnik: " + id);
                return NotFound("Ne postoji suradnik: " + id);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownLists();
                return View(partner);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                ProjectPartner partner = await ctx.ProjectPartner
                                  .Where(o => o.Id == id)
                                  .FirstOrDefaultAsync();
                if (partner == null)
                {
                    return NotFound("Neispravan id suradnika: " + id);
                }

                if (await TryUpdateModelAsync(partner, "",
                    o => o.Id, o => o.ProjectId, o => o.WorkerId, o => o.RoleId, o => o.DateFrom, o => o.DateTo
                ))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = $"Suradnik je ažuriran.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        await PrepareDropDownLists();
                        return View(partner);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o suradniku nije moguće povezati s forme");
                    await PrepareDropDownLists();
                    return View(partner);
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
            var partner = ctx.ProjectPartner.Find(Id);
            if (partner != null)
            {
                try
                {
                    ctx.Remove(partner);
                    ctx.SaveChanges();
                    logger.LogInformation($"Suradnik uspješno obrisan s projekta.");
                    TempData[Constants.Message] = $"Suradnik uspješno obrisan s projekta.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja suradnika s projekta: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja suradnika s projekta: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji traženi suradnik na odabranom projektu: ", Id);
                TempData[Constants.Message] = "Ne postoji traženi suradnik na odabranom projektu: " + Id;
                TempData[Constants.ErrorOccurred] = true;
            }

            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }
        private async Task PrepareDropDownLists()
        {
            var projects = await ctx.Project
                                 .ToListAsync();
            var workers = await ctx.Worker
                                 .ToListAsync();
            var roles = await ctx.ProjectRole
                                 .ToListAsync();


            var projectList = projects.Select(project => new SelectListItem
            {
                Text = $"{project.Name}",
                Value = project.Id.ToString()
            }).ToList();

            var workerList = workers.Select(worker => new SelectListItem
            {
                Text = $"{worker.Email}",
                Value = worker.Id.ToString()
            }).ToList();

            var roleList = roles.Select(role => new SelectListItem
            {
                Text = $"{role.Name}",
                Value = role.Id.ToString()
            }).ToList();


            ViewBag.Projects = projectList;
            ViewBag.Workers = workerList;
            ViewBag.ProjectRoles = roleList;
        }
    }
}
