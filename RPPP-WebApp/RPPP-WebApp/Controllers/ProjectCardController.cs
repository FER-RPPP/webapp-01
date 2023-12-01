using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Extensions;

namespace RPPP_WebApp.Controllers {
  public class ProjectCardController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<OwnerController> logger;
    private readonly AppSettings appData;

    public ProjectCardController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<OwnerController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

    public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.ProjectCard
                     .AsNoTracking();

      int count = await query.CountAsync();
      if (count == 0) {
        logger.LogInformation("Ne postoji niti jedna projektna kartica.");
        TempData[Constants.Message] = "Ne postoji niti jedna projektna kartica.";
        TempData[Constants.ErrorOccurred] = false;
        return RedirectToAction(nameof(Index));
      }

      var pagingInfo = new PagingInfo {
        CurrentPage = page,
        Sort = sort,
        Ascending = ascending,
        ItemsPerPage = pagesize,
        TotalItems = count
      };

      if (page < 1 || page > pagingInfo.TotalPages) {
        return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalPages, sort, ascending });
      }

      query = query.ApplySort(sort, ascending);

      var projectCard = await query
                  .Select(m => new ProjectCardViewModel {
                    Iban = m.Iban,
                    Balance = m.Balance,
                    ActivationDate = m.ActivationDate,
                    Owner = $"{m.OibNavigation.Name} {m.OibNavigation.Surname} ({m.OibNavigation.Oib})"
                  })
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToListAsync();

      var model = new ProjectCardsViewModel {
        ProjectCard = projectCard,
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
    public async Task<IActionResult> Create(ProjectCard projectCard) {
      if (ModelState.IsValid) {
        try {
          ctx.Add(projectCard);
          await ctx.SaveChangesAsync();

          TempData[Constants.Message] = $"Projektna kartica {projectCard.Iban} dodana.";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index));

        }
        catch (Exception exc) {
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          await PrepareDropDownLists();
          return View(projectCard);
        }
      }
      else {
        await PrepareDropDownLists();
        return View(projectCard);
      }
    }

    private async Task PrepareDropDownLists() {
      var owners = await ctx.Owner
                            .ToListAsync();

      var ownersList = owners.Select(owner => new SelectListItem {
        Text = $"{owner.Name} {owner.Surname} ({owner.Oib})",
        Value = owner.Oib.ToString()
      }).ToList();

      ViewBag.Owners = ownersList;
    }

  }
}
