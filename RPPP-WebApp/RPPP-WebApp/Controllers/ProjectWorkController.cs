using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Extensions;
using System.Text.Json;
using System.Security.Cryptography;
using NLog.Filters;

namespace RPPP_WebApp.Controllers {
  public class ProjectWorkController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<ProjectWorkController> logger;
    private readonly AppSettings appData;

    public ProjectWorkController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjectWorkController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

    public async Task<IActionResult> Index(ProjectWorkFilter filter, int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.ProjectWork
                     .AsNoTracking();

      string errorMessage = "Ne postoji niti jedna projektna aktivnost";
      if (!string.IsNullOrEmpty(filter.Title)) {
        query = query.Where(p => p.Title.Contains(filter.Title));
        errorMessage += $" gdje je naslov: {filter.Title}";
      }

      if (!string.IsNullOrEmpty(filter.Project)) {
        query = query.Where(p => p.Project.Name.Contains(filter.Project));
        errorMessage += $" gdje je naziv projekta: {filter.Project}";
      }

      if (!string.IsNullOrEmpty(filter.Assignee)) {
        query = query.Where(p => (p.Assignee.FirstName + p.Assignee.LastName).Contains(filter.Assignee));
        errorMessage += $" gdje je ime dodijeljenog radnika: {filter.Assignee}";
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

      var maxLength = 50;

      var projectWork = await query
                  .Select(o => new ProjectWorkViewModel {
                    Id = o.Id,
                    Title = o.Title,
                    Project = o.Project.Name,
                    Assignee = $"{o.Assignee.FirstName} {o.Assignee.LastName}",
                    Description = o.Description,
                    DiaryEntries = MakeShorter(string.Join(", ", o.LaborDiary.OrderBy(l => l.Date).Select(l => l.Date.ToString("dd.MM.yyyy."))), maxLength)
                  })
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToListAsync();

      var model = new ProjectWorksViewModel {
        ProjectWork = projectWork,
        PagingInfo = pagingInfo,
        Filter = filter,
      };

      return View(model);
    }

    public static string MakeShorter(string value, int maxLength) {
      if (value.Length <= maxLength)
        return value;
      else
        return value.Substring(0, maxLength) + "...";
    }


    public async Task<IActionResult> Show(Guid id, int page = 1, int sort = 1, bool ascending = true) {

      var query = ctx.LaborDiary
                     .AsNoTracking();
      query = query.ApplySort(sort, ascending);

      var diaryEntry = await query
                  .OrderBy(o => o.Date)
                  .Where(o => o.WorkId == id)
                  .Select(o => new LaborDiaryViewModel {
                    Work = o.Work.Title,
                    Worker = $"{o.Worker.FirstName} {o.Worker.LastName}",
                    Date = o.Date,
                    HoursSpent = o.HoursSpent,
                    LaborType = o.LaborType.Type,
                    LaborDescription = o.LaborDescription,
                  })
                  .ToListAsync();

      var projectWork = await ctx.ProjectWork
        .Where(o => o.Id == id)
        .Select(o => new ProjectWorkViewModel {
            Id = o.Id,
            Title = o.Title,
            Project = o.Project.Name,
            Assignee = $"{o.Assignee.FirstName} {o.Assignee.LastName}",
            Description = o.Description,
        })
        .FirstOrDefaultAsync();

      var model = new LaborDiariesViewModel {
        LaborDiary = diaryEntry
      };
        
      ViewData["Id"] = id;
      ViewData["WorkTitle"] = projectWork.Title;
      ViewData["Project"] = projectWork.Project;
      ViewData["Assignee"] = projectWork.Assignee;
      ViewData["Description"] = projectWork.Description;

      return View(model);
    }



    [HttpGet]
    public async Task<IActionResult> Create() {
      await PrepareDropDownLists();
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectWork projectWork) {
      if (ModelState.IsValid) {
        try {
          ctx.Add(projectWork);
          await ctx.SaveChangesAsync();
          logger.LogInformation(new EventId(1000), $"Projektna aktivnost {projectWork.Title} je dodana.");

          TempData[Constants.Message] = $"Projektna aktivnost {projectWork.Title} je dodana.";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index));

        }
        catch (Exception exc) {
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          await PrepareDropDownLists();
          return View(projectWork);
        }
      }
      else {
        await PrepareDropDownLists();
        return View(projectWork);
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      var projectWork = ctx.ProjectWork.Find(id);
      if (projectWork != null) {
        try {
          ctx.Remove(projectWork);
          ctx.SaveChanges();
          logger.LogInformation($"Projektna aktivnost {projectWork.Title} uspješno obrisana.");
          TempData[Constants.Message] = $"Projektna aktivnost {projectWork.Title} uspješno obrisana.";
          TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc) {
          TempData[Constants.Message] = "Pogreška prilikom brisanja projektne aktivnosti: " + exc.CompleteExceptionMessage();
          TempData[Constants.ErrorOccurred] = true;
          logger.LogError("Pogreška prilikom brisanja projektne aktivnosti: " + exc.CompleteExceptionMessage());
        }
      }
      else {
        logger.LogWarning("Ne postoji projektna aktivnost: " + id);
        TempData[Constants.Message] = "Ne postoji projektna aktivnost: " + id;
        TempData[Constants.ErrorOccurred] = true;
      }

      return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      var projectWork = ctx.ProjectWork.AsNoTracking().Where(o => o.Id == id).SingleOrDefault();
      if (projectWork == null) {
        logger.LogWarning("Ne postoji projektna aktivnost: " + id);
        return NotFound("Ne postoji projektna aktivnost: " + id);
      }
      else {
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        await PrepareDropDownLists();
        return View(projectWork);
      }
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      try {
        ProjectWork projectWork = await ctx.ProjectWork
                          .Where(o => o.Id == id)
                          .FirstOrDefaultAsync();
        if (projectWork == null) {
          return NotFound("Neispravan id projektne aktivnosti: " + id);
        }

        if (await TryUpdateModelAsync(projectWork, "",
            o => o.Title, o => o.ProjectId, o => o.AssigneeId, o => o.Description
        )) {
          ViewBag.Page = page;
          ViewBag.Sort = sort;
          ViewBag.Ascending = ascending;
          try {
            await ctx.SaveChangesAsync();
            TempData[Constants.Message] = $"Projektna aktivnost {projectWork.Title} je ažurirana.";
            TempData[Constants.ErrorOccurred] = false;
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
          }
          catch (Exception exc) {
            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
            await PrepareDropDownLists();
            return View(projectWork);
          }
        }
        else {
          ModelState.AddModelError(string.Empty, "Podatke o projektnoj aktivnosti nije moguće povezati s forme");
          await PrepareDropDownLists();
          return View(projectWork);
        }
      }
      catch (Exception exc) {
        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        TempData[Constants.ErrorOccurred] = true;
        return RedirectToAction(nameof(Edit), id);
      }
    }
    private async Task PrepareDropDownLists() {
      var projects = await ctx.Project
                                .ToListAsync();

      var assignees = await ctx.Worker
                            .ToListAsync();

      var projectList = projects.Select(project => new SelectListItem
      {
        Text = $"{project.Name}",
        Value = project.Id.ToString()
      }).ToList();

      var assigneeList = assignees.Select(assignee => new SelectListItem
      {
        Text = $"{assignee.FirstName} {assignee.LastName}",
        Value = assignee.Id.ToString()
      }).ToList();

      ViewBag.Projects = projectList;
      ViewBag.Assignees = assigneeList;
    }

  }
}
