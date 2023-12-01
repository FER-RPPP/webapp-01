﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Extensions;
using System.Text.Json;
using System.Security.Cryptography;

namespace RPPP_WebApp.Controllers {
  public class ProjectCardController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<ProjectCardController> logger;
    private readonly AppSettings appData;

    public ProjectCardController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjectCardController> logger) {
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
        return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
      }

      query = query.ApplySort(sort, ascending);

      var projectCard = await query
                  .Select(o => new ProjectCardViewModel {
                    Iban = o.Iban,
                    Balance = o.Balance,
                    ActivationDate = o.ActivationDate,
                    Owner = $"{o.OibNavigation.Name} {o.OibNavigation.Surname} ({o.OibNavigation.Oib})"
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(string Iban, int page = 1, int sort = 1, bool ascending = true) {
      var projectCard = ctx.ProjectCard.Find(Iban);
      if (projectCard != null) {
        try {
          ctx.Remove(projectCard);
          ctx.SaveChanges();
          logger.LogInformation($"Projektna kartica IBAN = {Iban} uspješno obrisana.");
          TempData[Constants.Message] = $"Projektna kartica IBAN = {Iban} uspješno obrisana.";
          TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc) {
          TempData[Constants.Message] = "Pogreška prilikom brisanja projektne kartice: " + exc.CompleteExceptionMessage();
          TempData[Constants.ErrorOccurred] = true;
          logger.LogError("Pogreška prilikom brisanja projektne kartice: " + exc.CompleteExceptionMessage());
        }
      }
      else {
        logger.LogWarning("Ne postoji projektna kartica s IBAN-om: {0} ", Iban);
        TempData[Constants.Message] = "Ne postoji projektna kartica s IBAN-om: " + Iban;
        TempData[Constants.ErrorOccurred] = true;
      }

      return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, int page = 1, int sort = 1, bool ascending = true) {
      var projectCard = ctx.ProjectCard.AsNoTracking().Where(o => o.Iban == id).SingleOrDefault();
      if (projectCard == null) {
        logger.LogWarning("Ne postoji projektna kartica s IBAN-om: " + id);
        return NotFound("Ne postoji projektna kartica s IBAN-om: " + id);
      }
      else {
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        await PrepareDropDownLists();
        return View(projectCard);
      }
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string id, int page = 1, int sort = 1, bool ascending = true) {
      try {
        ProjectCard projectCard = await ctx.ProjectCard
                          .Where(o => o.Iban == id)
                          .FirstOrDefaultAsync();
        if (projectCard == null) {
          return NotFound("Neispravan IBAN projektne kartice: " + id);
        }

        if (await TryUpdateModelAsync(projectCard, "",
            o => o.Iban, o => o.Balance, o => o.ActivationDate, o => o.Oib
        )) {
          ViewBag.Page = page;
          ViewBag.Sort = sort;
          ViewBag.Ascending = ascending;
          try {
            await ctx.SaveChangesAsync();
            TempData[Constants.Message] = $"Projektna kartica (IBAN = {id}) ažurirana.";
            TempData[Constants.ErrorOccurred] = false;
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
          }
          catch (Exception exc) {
            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
            await PrepareDropDownLists();
            return View(projectCard);
          }
        }
        else {
          ModelState.AddModelError(string.Empty, "Podatke o projektnoj kartici nije moguće povezati s forme");
          await PrepareDropDownLists();
          return View(projectCard);
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

      var ownersList = owners.Select(owner => new SelectListItem {
        Text = $"{owner.Name} {owner.Surname} ({owner.Oib})",
        Value = owner.Oib.ToString()
      }).ToList();

      ViewBag.Owners = ownersList;
    }

  }
}