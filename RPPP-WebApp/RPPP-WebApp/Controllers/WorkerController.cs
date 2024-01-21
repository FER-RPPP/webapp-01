using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Common;
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
                    worker.Id = Guid.NewGuid();
                    ctx.Add(worker);
                  
                    await ctx.SaveChangesAsync();
                    logger.LogInformation("Radnik je dodan.");
                    TempData[Constants.Message] = $"Radnik je dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    logger.LogInformation("Pogreška pri dodavanju radnika.");
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
        [HttpGet]
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
                    logger.LogWarning("Neispravan id radnika: " + id);
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
                        logger.LogInformation("Radnik je ažuriran.");
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
                logger.LogWarning("Ne postoji radnik: " + Id);
                TempData[Constants.Message] = "Ne postoji radnik: " + Id;
                TempData[Constants.ErrorOccurred] = true;
            }

            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }

        public async Task<IActionResult> Details(Guid id, int page = 1, int sort = 1, bool ascending = true)
        {

            var query = ctx.ProjectPartner
                           .AsNoTracking();
            query = query.ApplySort(sort, ascending);

            var projectEntry = await query
              .Where(o => o.WorkerId == id)
              .Select(o => new ProjectPartnerViewModel
              {
                  Id = o.Id,
                  Project = o.Project.Name,
                  Role = o.Role.Name,
                  DateFrom = o.DateFrom,
                  DateTo = o.DateTo
              })
              .ToListAsync();
              

            var worker = await ctx.Worker
            .Where(o => o.Id == id)
            .Select(o => new WorkerViewModel
            {
                Email = o.Email,
                FirstName = o.FirstName,
                LastName = o.LastName,
                PhoneNumber = o.PhoneNumber,
                Organization = o.Organization.Name
            })
            .FirstOrDefaultAsync();
            var model = new ProjectPartnersViewModel
            {
                Partners = projectEntry
            };

            ViewData["Id"] = id;
            ViewData["Worker"] = $"{worker.FirstName} {worker.LastName}";
            ViewData["Email"] = worker.Email;
            ViewData["Phone"] = worker.PhoneNumber;
            ViewData["Organization"] = worker.Organization;

            return View(model);
        }
        public async Task<IActionResult> GetProjectPartner(Guid id)
        {
            var partner = await ctx.ProjectPartner
                                       .Where(o => o.Id == id)
                                       .Select(o => new ProjectPartnerViewModel
                                       {
                                           Id = o.Id,
                                           Project = o.Project.Name,
                                           Role = o.Role.Name,
                                           DateFrom = o.DateFrom,
                                           DateTo = o.DateTo

                                       })
                                       .FirstOrDefaultAsync();

            if (partner != null)
            {
                logger.LogWarning("Nema poslanih podataka");
                return PartialView(partner);
            }
            else
            {
                logger.LogWarning("Neispravan id suradnika" + id);
                return NotFound($"Neispravan id suradnika!: {id}");
            }

        }



        public async Task<IActionResult> EditProjectPartner(Guid id)
        {
            var partner = await ctx.ProjectPartner
                           .Where(o => o.Id == id)
                           .Select(o => new ProjectPartnerViewModel
                           {
                               Id = o.Id,
                               Project = o.Project.Name,
                               Role = o.Role.Name,
                               DateFrom = o.DateFrom,
                               DateTo = o.DateTo

                           })
                           .FirstOrDefaultAsync();

            await PrepareDropDownLists();
            if (partner != null)
            {
                logger.LogWarning("Nema poslanih podataka");
                return PartialView(partner);
            }
            else
            {
                logger.LogWarning("Neispravan id suradnika" + id);
                return NotFound($"Neispravan id suradnika: {id}");
            }

        }
        [HttpPost]
        public async Task<IActionResult> EditProjectPartner(ProjectPartnerViewModel partner)
        {
            if (partner == null)
            {
                logger.LogWarning("Nema poslanih podataka");
                return NotFound("Nema poslanih podataka");
            }
            ProjectPartner dbPartner = await ctx.ProjectPartner.FindAsync(partner.Id);
            if (dbPartner == null)
            {
                logger.LogWarning("Neispravan id suradnika");
                return NotFound($"Neispravan id suradnika: {partner.Id}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    dbPartner.Id = partner.Id;
                    Guid roleId = new Guid(partner.Role);
                    dbPartner.RoleId = roleId;
                    dbPartner.DateFrom = partner.DateFrom;
                    dbPartner.DateTo = partner.DateTo;


                    await ctx.SaveChangesAsync();

                    logger.LogWarning("Uspješna izmjena podataka o suradnji");
                    return RedirectToAction(nameof(GetProjectPartner), new { id = partner.Id });
                }
                catch (Exception exc)
                {
                    logger.LogWarning("Pogreška pri izmjeni podataka o suradnji");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(partner);
                }
            }
            else
            {
                return PartialView(partner);
            }
        }
        public async Task<IActionResult> DeleteProjectPartner(Guid id)
        {

            var partner = ctx.ProjectPartner.Find(id);
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
                logger.LogWarning("Ne postoji traženi suradnik na odabranom projektu: ", id);
                TempData[Constants.Message] = "Ne postoji traženi suradnik na odabranom projektu: " + id;
                TempData[Constants.ErrorOccurred] = true;
            }

            return TempData[Constants.ErrorOccurred].Equals(false) ? new EmptyResult() : await GetProjectPartner(id);

        }
        [HttpGet]
        public async Task<IActionResult> AddProjectPartner()
        {
            await PrepareDropDownLists();
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> AddProjectPartner(ProjectPartner partner)
        {
            if (!ModelState.IsValid)
            {
                await PrepareDropDownLists();
                return PartialView();
            }

            var newPartner = new ProjectPartner
            {
                Id = Guid.NewGuid(),
                Project = partner.Project,
                Role = partner.Role,
                DateFrom = partner.DateFrom,
                DateTo = partner.DateTo,
                Worker = partner.Worker
                
            };

            try
            {
                ctx.Add(newPartner);
                await ctx.SaveChangesAsync();
                logger.LogInformation("Suradnja je dodana.");
                TempData["Message"] = "Suradnja je dodana.";
                TempData["ErrorOccurred"] = false;
                return RedirectToAction(nameof(Details));
            }
            catch (Exception exc)
            {
                logger.LogInformation("Pogreška pri dodavanju suradnje.");
                ModelState.AddModelError(string.Empty, exc.Message);
                await PrepareDropDownLists();
                return PartialView(partner);
            }

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

            var projects = await ctx.Project
                     .ToListAsync();
            var roles = await ctx.ProjectRole
                                 .ToListAsync();


            var projectList = projects.Select(project => new SelectListItem
            {
                Text = $"{project.Name}",
                Value = project.Id.ToString()
            }).ToList();

            var roleList = roles.Select(role => new SelectListItem
            {
                Text = $"{role.Name}",
                Value = role.Id.ToString()
            }).ToList();


            ViewBag.Projects = projectList;
            ViewBag.ProjectRoles = roleList;
        }
    }
}
