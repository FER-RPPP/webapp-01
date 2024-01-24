using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RPPP_WebApp.Controllers {
    /// <summary>
    /// Controller for managing projects (domain 4 version).
    /// </summary>
    public class ProjectMD4Controller : Controller {
		private readonly Rppp01Context ctx;
    private readonly ILogger<ProjectMD4Controller> logger;
    private readonly AppSettings appData;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectMD4Controller"/> class.
    /// </summary>
    /// <param name="ctx">The database context.</param>
    /// <param name="options">The application settings options.</param>
    /// <param name="logger">The logger.</param>
    public ProjectMD4Controller(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjectMD4Controller> logger) {
	        this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
		}

    /// <summary>
    /// Displays a paginated list of projects.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>The view displaying the paginated list of projects.</returns>
    public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.Project
                     .AsNoTracking();

      int count = await query.CountAsync();
      if (count == 0) {
        logger.LogInformation("Ne postoji niti jedan projekt.");
        TempData[Constants.Message] = "Ne postoji niti jedan projekt.";
        TempData[Constants.ErrorOccurred] = false;
        return RedirectToAction(nameof(Create));
      }

      var pagingInfo = new PagingInfo {
        CurrentPage = page,
        Sort = sort,
        Ascending = ascending,
        ItemsPerPage = pagesize,
        TotalItems = count
      };

      if (page < 1 || page > pagingInfo.TotalPages) {
        return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
      }

      query = query.ApplySort(sort, ascending);

      var project = await query
                  .Select(o => new ProjectMD4ViewModel {
                    Id = o.Id,
                    Name = o.Name,
                    Type = o.Type,
                    Owner = $"{o.Owner.Name} {o.Owner.Surname} ({o.Owner.Oib})",
                    Client = $"{o.Client.FirstName} {o.Client.LastName} ({o.Client.Oib})",
                    Iban = o.CardId,
                  })
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToListAsync();

      var model = new ProjectsMD4ViewModel {
        ProjectMD4 = project,
        PagingInfo = pagingInfo
      };

      return View(model);
		}

    /// <summary>
    /// Displays the view for creating a new project.
    /// </summary>
    /// <returns>The result of the action.</returns>
    [HttpGet]
    public async Task<IActionResult> Create() {
      await PrepareDropDownLists();
      return View();
    }

    /// <summary>
    /// Handles the HTTP POST request for creating a new project.
    /// </summary>
    /// <param name="project">The project to be created.</param>
    /// <returns>The result of the action.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project) {
      logger.LogTrace(JsonSerializer.Serialize(project));
      if (ModelState.IsValid) {
        try {
          project.Id = Guid.NewGuid();
          ctx.Add(project);
          await ctx.SaveChangesAsync();
          logger.LogInformation(new EventId(1000), $" Projekt {project.Name} je dodan.");

          TempData[Constants.Message] = $"Projekt {project.Name} je dodan.";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index));
        }
        catch (Exception exc) {
          logger.LogError("Pogreška prilikom dodavanja novog projekta: " + exc.CompleteExceptionMessage());
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          await PrepareDropDownLists();
          return View(project);
        }
      }
      else {
         await PrepareDropDownLists();
         return View(project);
      }
    }

    /// <summary>
    /// Handles the HTTP POST request for deleting a project.
    /// </summary>
    /// <param name="id">The ID of the project to be deleted.</param>
    /// <param name="page">The page number.</param>
    /// <param name="sort">The sort option.</param>
    /// <param name="ascending">The sort direction.</param>
    /// <returns>The result of the action.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      var project = ctx.Project.Find(id);
      if (project != null) {
        try {
          ctx.Remove(project);
          ctx.SaveChanges();
          logger.LogInformation($"Projekt {project.Name} uspješno obrisan.");
          TempData[Constants.Message] = $"Projekt {project.Name} uspješno obrisan.";
          TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc) {
          TempData[Constants.Message] = "Pogreška prilikom brisanja projekta: " + exc.CompleteExceptionMessage();
          TempData[Constants.ErrorOccurred] = true;
          logger.LogError("Pogreška prilikom brisanja projekta: " + exc.CompleteExceptionMessage());
        }
      }
      else {
        logger.LogWarning("Ne postoji projekt: " + id);
        TempData[Constants.Message] = "Ne postoji projekt: " + id;
        TempData[Constants.ErrorOccurred] = true;
      }
      return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
    }

    /// <summary>
    /// Displays the view for editing an existing project.
    /// </summary>
    /// <param name="id">The ID of the project to be edited.</param>
    /// <param name="page">The page number.</param>
    /// <param name="sort">The sort option.</param>
    /// <param name="ascending">The sort direction.</param>
    /// <returns>The result of the action.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      var project = ctx.Project.AsNoTracking().Where(o => o.Id == id).SingleOrDefault();
      if (project == null) {
        logger.LogWarning("Ne postoji projekt: " + id);
        return NotFound("Ne postoji projekt: " + id);
      }
      else {
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        await PrepareDropDownLists();
        return View(project);
      }
    }

    /// <summary>
    /// Handles the HTTP POST request for updating an existing project.
    /// </summary>
    /// <param name="id">The ID of the project to be updated.</param>
    /// <param name="page">The page number.</param>
    /// <param name="sort">The sort option.</param>
    /// <param name="ascending">The sort direction.</param>
    /// <returns>The result of the action.</returns>
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      try {
        Project project = await ctx.Project
                          .Where(o => o.Id == id)
                          .FirstOrDefaultAsync();
        if (project == null) {
          return NotFound("Neispravan id projekta: " + id);
        }

        if (await TryUpdateModelAsync<Project>(project, "",
            o => o.Id, o => o.Name, o => o.Type, o => o.OwnerId, o => o.ClientId, o => o.CardId
        )) {
          ViewBag.Page = page;
          ViewBag.Sort = sort;
          ViewBag.Ascending = ascending;
          try {
            await ctx.SaveChangesAsync();
            TempData[Constants.Message] = $"Projekt {project.Name} je ažuriran.";
            TempData[Constants.ErrorOccurred] = false;
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
          }
          catch (Exception exc) {
            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
            await PrepareDropDownLists();
            return View(project);
          }
        }
        else {
          ModelState.AddModelError(string.Empty, "Podatke o projektu nije moguće povezati s forme");
          await PrepareDropDownLists();
          return View(project);
        }
      }
      catch (Exception exc) {
        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        TempData[Constants.ErrorOccurred] = true;
        return RedirectToAction(nameof(Edit), id);
      }
    }

    /// <summary>
    /// Prepares dropdown lists for form fields.
    /// </summary>
    /// <returns>Task representing the asynchronous operation.</returns>
    private async Task PrepareDropDownLists() {
    var projects = await ctx.Owner
                            .ToListAsync();

    var clients = await ctx.Client
                        .ToListAsync();

    var cards = await ctx.ProjectCard
                      .ToListAsync();

    var projectList = projects.Select(project => new SelectListItem {
        Text = $"{project.Name} {project.Surname} ({project.Oib})",
        Value = project.Oib
    }).ToList();

    var clientList = clients.Select(client => new SelectListItem {
        Text = $"{client.FirstName} {client.LastName} ({client.Oib})",
        Value = client.Id.ToString()
    }).ToList();

    var cardList = cards.Select(card => new SelectListItem {
        Text = card.Iban,
        Value = card.Iban
    }).ToList();

    ViewBag.Owners = projectList;
    ViewBag.Clients = clientList;
    ViewBag.Cards = cardList;
    }
  }
}




