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
    /// <summary>
    /// Controller for managing roles on project.
    /// </summary>
    public class ProjectRoleController : Controller
    {
        private readonly Rppp01Context ctx;
        private readonly ILogger<TransactionTypeController> logger;
        private readonly AppSettings appData;


        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectRoleController"/> class.
        /// </summary>
        /// <param name="ctx">The database context.</param>
        /// <param name="options">Application settings.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>A new instance of the <see cref="ProjectRoleController"/> class.</returns>
        public ProjectRoleController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<TransactionTypeController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appData = options.Value;
        }

        /// <summary>
        /// Displays a paginated list of project roles.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="sort">The sort option.</param>
        /// <param name="ascending">The sort direction.</param>
        /// <returns>The result of the action.</returns>
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

        /// <summary>
        /// Displays the view for creating a new role.
        /// </summary>
        /// <returns>The result of the action.</returns>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Handles the HTTP POST request for adding a new role.
        /// </summary>
        /// <param name="projectRole">The role data from the form.</param>
        /// <returns>Redirects to the role index on success; returns the form on failure.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProjectRole projectRole)
        {
            logger.LogTrace(JsonSerializer.Serialize(projectRole));
            if (ModelState.IsValid)
            {
                try
                {
                    projectRole.Id = Guid.NewGuid();
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
        /// <summary>
        /// Opens the form for editing data about an existing role.
        /// </summary>
        /// <param name="Id">The unique identifier of the role to edit.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Role edit view.</returns>
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
        /// <summary>
        /// Handles the HTTP POST request for updating data about an existing role.
        /// </summary>
        /// <param name="Id">The unique identifier of the role to be updated.</param>
        /// <param name="page">The current page number.</param>
        /// <param name="sort">The sort order.</param>
        /// <param name="ascending">Whether the sorting is in ascending order.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns the edit view with error messages.</returns>
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


        /// <summary>
        /// Deletes a role with the specified id.
        /// </summary>
        /// <param name="Id">The unique identifier of the role to remove.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Sort order.</param>
        /// <param name="ascending">Whether sorting is in ascending order.</param>
        /// <returns>Redirects to an updated index view.</returns>
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
