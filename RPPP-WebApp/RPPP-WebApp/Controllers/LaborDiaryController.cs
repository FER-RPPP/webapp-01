using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Extensions;

namespace RPPP_WebApp.Controllers {
  public class LaborDiaryController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<LaborDiaryController> logger;
    private readonly AppSettings appData;

    public LaborDiaryController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<LaborDiaryController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

    public async Task<IActionResult> Index(LaborDiaryFilter filter, int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.LaborDiary
                     .AsNoTracking();

      string errorMessage = "Ne postoji niti jedna transakcija";

      if (!string.IsNullOrEmpty(filter.Type)) {
        query = query.Where(p => p.LaborType.Type.Contains(filter.Type));   // NOTE SI TU OSTAVLJAM
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

    
    [HttpGet]
    public async Task<IActionResult> Create() {
      await PrepareDropDownLists();
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LaborDiary laborDiary) {
      if (ModelState.IsValid) {
        try {
          ctx.Add(laborDiary);
          await ctx.SaveChangesAsync();

          TempData[Constants.Message] = $"Transakcija je dodana.";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index));

        }
        catch (Exception exc) {
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid Id, int page = 1, int sort = 1, bool ascending = true) {
      var laborDiary = ctx.LaborDiary.Find(Id);
      if (laborDiary != null) {
        try {
          ctx.Remove(laborDiary);
          ctx.SaveChanges();
          logger.LogInformation($"Transakcija uspješno obrisana.");
          TempData[Constants.Message] = $"Transakcija uspješno obrisana.";
          TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc) {
          TempData[Constants.Message] = "Pogreška prilikom brisanja transakcije: " + exc.CompleteExceptionMessage();
          TempData[Constants.ErrorOccurred] = true;
          logger.LogError("Pogreška prilikom brisanja transakcije: " + exc.CompleteExceptionMessage());
        }
      }
      else {
        logger.LogWarning("Ne postoji transakcija: ", Id);
        TempData[Constants.Message] = "Ne postoji transakcija: " + Id;
        TempData[Constants.ErrorOccurred] = true;
      }

      return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      var laborDiary = ctx.LaborDiary.AsNoTracking().Where(o => o.Id == id).SingleOrDefault();
      if (laborDiary == null) {
        logger.LogWarning("Ne postoji transakcija: " + id);
        return NotFound("Ne postoji transakcija: " + id);
      }
      else {
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        await PrepareDropDownLists();
        return View(laborDiary);
      }
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      try {
        LaborDiary laborDiary = await ctx.LaborDiary
                          .Where(o => o.Id == id)
                          .FirstOrDefaultAsync();
        if (laborDiary == null) {
          return NotFound("Neispravan id transakcije: " + id);
        }

        if (await TryUpdateModelAsync(laborDiary, "",
            o => o.Id, o => o.WorkId, o => o.WorkerId, o => o.Date, o => o.HoursSpent, o => o.LaborTypeId, o => o.LaborDescription
        )) {
          ViewBag.Page = page;
          ViewBag.Sort = sort;
          ViewBag.Ascending = ascending;
          try {
            await ctx.SaveChangesAsync();
            TempData[Constants.Message] = $"Transakcija je ažurirana.";
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
          ModelState.AddModelError(string.Empty, "Podatke o transakciji nije moguće povezati s forme");
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

