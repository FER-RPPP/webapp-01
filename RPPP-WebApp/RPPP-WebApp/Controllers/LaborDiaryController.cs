﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Extensions;

namespace RPPP_WebApp.Controllers {
  /// <summary>
  /// Controller for managing labor diary entries.
  /// </summary>
  public class LaborDiaryController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<LaborDiaryController> logger;
    private readonly AppSettings appData;

    /// <summary>
    /// Initializes a new instance of the <see cref="LaborDiaryController"/> class.
    /// </summary>
    /// <param name="ctx">The database context.</param>
    /// <param name="options">Application settings.</param>
    /// <param name="logger">Logger instance.</param>
    public LaborDiaryController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<LaborDiaryController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

    /// <summary>
    /// Displays a paginated list of labor diary entries based on filter criteria.
    /// </summary>
    /// <param name="filter">Filter criteria.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>The view displaying the paginated list of labor diary entries.</returns>
    public async Task<IActionResult> Index(LaborDiaryFilter filter, int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.LaborDiary
                     .AsNoTracking();

      string errorMessage = "Ne postoji niti jedan zapis";

      if (!string.IsNullOrEmpty(filter.Type)) {
        query = query.Where(p => p.LaborType.Type.Contains(filter.Type));
        errorMessage += $" gdje je vrsta: {filter.Type}";
      }

      int count = await query.CountAsync();
      if (count == 0) {
        logger.LogInformation(errorMessage);
        TempData[Constants.Message] = errorMessage;
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

      var laborDiary = await query
                  .Select(o => new LaborDiaryViewModel {
                    Id = o.Id,
                    Work = o.Work.Title,
                    Worker = $"{o.Worker.FirstName} {o.Worker.LastName}",
                    Date = o.Date,
                    HoursSpent = o.HoursSpent,
                    LaborType = o.LaborType.Type,
                    LaborDescription = o.LaborDescription,
                  })
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToListAsync();

      var model = new LaborDiariesViewModel {
        LaborDiary = laborDiary,
        PagingInfo = pagingInfo,
        Filter = filter
      };

      return View(model);
    }

    /// <summary>
    /// Displays the form for creating a new labor diary entry.
    /// </summary>
    /// <returns>The view for creating a new labor diary entry.</returns>    
    [HttpGet]
    public async Task<IActionResult> Create() {
      await PrepareDropDownLists();
      return View();
    }

    /// <summary>
    /// Handles the submission of the new labor diary entry form.
    /// </summary>
    /// <param name="laborDiary">The labor diary entry data from the form.</param>
    /// <returns>Redirects to the labor diary entry index on success; returns the form on failure.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LaborDiary laborDiary) {
      if (ModelState.IsValid) {
        try {
          ctx.Add(laborDiary);
          await ctx.SaveChangesAsync();
          logger.LogInformation(new EventId(1000), $"Zapis {laborDiary.Date} - {laborDiary.Worker.FirstName} {laborDiary.Worker.LastName} je dodan.");

          TempData[Constants.Message] = $"Zapis {laborDiary.Date} - {laborDiary.Worker.FirstName} {laborDiary.Worker.LastName} je dodan.";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index));

        }
        catch (Exception exc) {
          logger.LogError("Pogreška prilikom dodavanja novog zapisa: " + exc.CompleteExceptionMessage());
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          await PrepareDropDownLists();
          return View(laborDiary);
        }
      }
      else {
        await PrepareDropDownLists();
        return View(laborDiary);
      }
    }

        /// <summary>
        /// Deletes a labor diary entry based on its ID.
        /// </summary>
        /// <param name="id">The ID of the labor diary entry to be deleted.</param>
        /// <param name="page">Page number.</param>
        /// <param name="sort">Sorting option.</param>
        /// <param name="ascending">Sort order.</param>
        /// <returns>Redirects to the labor diary entry index after deletion.</returns>
        [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      var laborDiary = ctx.LaborDiary.Find(id);
      if (laborDiary != null) {
        try {
          ctx.Remove(laborDiary);
          ctx.SaveChanges();
          logger.LogInformation($"Zapis {laborDiary.Date} - {laborDiary.Worker.FirstName} {laborDiary.Worker.LastName} uspješno obrisan.");
          TempData[Constants.Message] = $"Zapis {laborDiary.Date} - {laborDiary.Worker.FirstName} {laborDiary.Worker.LastName} uspješno obrisan.";
          TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc) {
          TempData[Constants.Message] = "Pogreška prilikom brisanja zapisa: " + exc.CompleteExceptionMessage();
          TempData[Constants.ErrorOccurred] = true;
          logger.LogError("Pogreška prilikom brisanja zapisa: " + exc.CompleteExceptionMessage());
        }
      }
      else {
        logger.LogWarning("Ne postoji zapis: " + id);
        TempData[Constants.Message] = "Ne postoji zapis: " + id;
        TempData[Constants.ErrorOccurred] = true;
      }

      return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
    }

    /// <summary>
    /// Displays the form for editing an existing labor diary entry.
    /// </summary>
    /// <param name="id">The ID of the labor diary entry to be edited.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>The view for editing an existing labor diary entry.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      var laborDiary = ctx.LaborDiary.AsNoTracking().Where(o => o.Id == id).SingleOrDefault();
      if (laborDiary == null) {
        logger.LogWarning("Ne postoji zapis: " + id);
        return NotFound("Ne postoji zapis: " + id);
      }
      else {
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        await PrepareDropDownLists();
        return View(laborDiary);
      }
    }

    /// <summary>
    /// Handles the submission of the edited labor diary entry form.
    /// </summary>
    /// <param name="id">The ID of the labor diary entry to be updated.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>Redirects to the labor diary entry index on success; returns the form on failure.</returns>
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      try {
        LaborDiary laborDiary = await ctx.LaborDiary
                          .Where(o => o.Id == id)
                          .FirstOrDefaultAsync();
        if (laborDiary == null) {
          return NotFound("Neispravan id zapisa: " + id);
        }

        if (await TryUpdateModelAsync(laborDiary, "",
            o => o.Id, o => o.WorkId, o => o.WorkerId, o => o.Date, o => o.HoursSpent, o => o.LaborTypeId, o => o.LaborDescription
        )) {
          ViewBag.Page = page;
          ViewBag.Sort = sort;
          ViewBag.Ascending = ascending;
          try {
            await ctx.SaveChangesAsync();
            TempData[Constants.Message] = $"Zapis {laborDiary.Date} - {laborDiary.Worker.FirstName} {laborDiary.Worker.LastName} je ažuriran.";
            TempData[Constants.ErrorOccurred] = false;
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
          }
          catch (Exception exc) {
            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
            await PrepareDropDownLists();
            return View(laborDiary);
          }
        }
        else {
          ModelState.AddModelError(string.Empty, "Podatke o zapisu nije moguće povezati s forme");
          await PrepareDropDownLists();
          return View(laborDiary);
        }
      }
      catch (Exception exc) {
        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        TempData[Constants.ErrorOccurred] = true;
        return RedirectToAction(nameof(Edit), new { id = id, page = page, sort = sort, ascending = ascending });
      }
    }

    /// <summary>
    /// Prepares dropdown lists for form fields.
    /// </summary>
    /// <returns>Task representing the asynchronous operation.</returns>
    private async Task PrepareDropDownLists() {
      var works = await ctx.ProjectWork
                           .ToListAsync();

      var workers = await ctx.Worker
                          .ToListAsync();

      var laborTypes = await ctx.LaborType
                          .ToListAsync();

      var workList = works.Select(work => new SelectListItem {
        Text = $"{work.Title}",
        Value = work.Id.ToString()
      }).ToList();

      var workerList = workers.Select(worker => new SelectListItem {
        Text = $"{worker.FirstName} {worker.LastName}",
        Value = worker.Id.ToString()
      }).ToList();

      var laborTypeList = laborTypes.Select(type => new SelectListItem {
        Text = $"{type.Type}",
        Value = type.Id.ToString()
      }).ToList();

      ViewBag.Works = workList;
      ViewBag.Workers = workerList;
      ViewBag.LaborTypes = laborTypeList;
    }
    
  }
}

