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
	public class ProjectMD4Controller : Controller {
		private readonly Rppp01Context ctx;
    private readonly ILogger<ProjectMD4Controller> logger;
    private readonly AppSettings appData;

		public ProjectMD4Controller(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjectMD4Controller> logger) {
			this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
		}

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

    [HttpGet]
    public async Task<IActionResult> Create() {
      await PrepareDropDownLists();
      return View();
    }

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

    private async Task PrepareDropDownLists() {
    var owners = await ctx.Owner
                            .ToListAsync();

    var clients = await ctx.Client
                        .ToListAsync();

    var cards = await ctx.ProjectCard
                      .ToListAsync();

    var ownerList = owners.Select(owner => new SelectListItem {
        Text = $"{owner.Name} {owner.Surname} ({owner.Oib})",
        Value = owner.Oib
    }).ToList();

    var clientList = clients.Select(client => new SelectListItem {
        Text = $"{client.FirstName} {client.LastName} ({client.Oib})",
        Value = client.Id.ToString()
    }).ToList();

    var cardList = cards.Select(card => new SelectListItem {
        Text = card.Iban,
        Value = card.Iban
    }).ToList();

    ViewBag.Owners = ownerList;
    ViewBag.Clients = clientList;
    ViewBag.Cards = cardList;
    }
  }
}




