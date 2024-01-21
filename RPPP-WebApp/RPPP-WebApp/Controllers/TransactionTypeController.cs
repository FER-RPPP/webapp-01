using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers {
  public class TransactionTypeController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<TransactionTypeController> logger;
    private readonly AppSettings appData;

    public TransactionTypeController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<TransactionTypeController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.TransactionType
                     .AsNoTracking();

      int count = query.Count();
      if (count == 0) {
        logger.LogInformation("Ne postoji niti jedna vrsta transakcije.");
        TempData[Constants.Message] = "Ne postoji niti jedna vrsta transakcije.";
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

      var type = query
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToList();

      var model = new TransactionTypeViewModel {
        TransactionType = type,
        PagingInfo = pagingInfo
      };

      return View(model);
    }


    [HttpGet]
    public IActionResult Create() {
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TransactionType transactionType) {
      logger.LogTrace(JsonSerializer.Serialize(transactionType));
      if (ModelState.IsValid) {
        try {
          ctx.Add(transactionType);
          ctx.SaveChanges();
          logger.LogInformation(new EventId(1000), $"Vrsta {transactionType.TypeName} je dodana.");

          TempData[Constants.Message] = $"Vrsta {transactionType.TypeName} je dodana";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index));
        }
        catch (Exception exc) {
          logger.LogError("Pogreška prilikom dodavanja nove vrste: {0}", exc.CompleteExceptionMessage());
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          return View(transactionType);
        }
      }
      else {
        return View(transactionType);
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid Id, int page = 1, int sort = 1, bool ascending = true) {
      var transactionType = ctx.TransactionType.Find(Id);
      if (transactionType != null) {
        try {
          ctx.Remove(transactionType);
          ctx.SaveChanges();
          logger.LogInformation($"Vrsta {transactionType.TypeName} uspješno obrisana.");
          TempData[Constants.Message] = $"Vrsta {transactionType.TypeName} uspješno obrisana.";
          TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc) {
          TempData[Constants.Message] = "Pogreška prilikom brisanja vrste: " + exc.CompleteExceptionMessage();
          TempData[Constants.ErrorOccurred] = true;
          logger.LogError("Pogreška prilikom brisanja vrste: " + exc.CompleteExceptionMessage());
        }
      }
      else {
        logger.LogWarning("Ne postoji vrsta");
        TempData[Constants.Message] = "Ne postoji vrsta";
        TempData[Constants.ErrorOccurred] = true;
      }
      return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
    }


    [HttpGet]
    public IActionResult Edit(Guid Id, int page = 1, int sort = 1, bool ascending = true) {
      var transactionType = ctx.TransactionType.AsNoTracking().Where(o => o.Id == Id).SingleOrDefault();
      if (transactionType == null) {
        logger.LogWarning("Ne postoji ta vrsta");
        return NotFound("Ne postoji ta vrsta");
      }
      else {
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        return View(transactionType);
      }
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid Id, int page = 1, int sort = 1, bool ascending = true) {
      try {
        TransactionType transactionType = await ctx.TransactionType
                          .Where(o => o.Id == Id)
                          .FirstOrDefaultAsync();
        if (transactionType == null) {
          return NotFound("Neispravan id vrste");
        }

        if (await TryUpdateModelAsync<TransactionType>(transactionType, "",
            o => o.TypeName, o => o.Id
        )) {
          ViewBag.Page = page;
          ViewBag.Sort = sort;
          ViewBag.Ascending = ascending;
          try {
            await ctx.SaveChangesAsync();
            logger.LogInformation($"Vrsta {transactionType.TypeName} ažurirana.");
            TempData[Constants.Message] = $"Vrsta {transactionType.TypeName} ažurirana.";
            TempData[Constants.ErrorOccurred] = false;
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
          }
          catch (Exception exc) {
            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
            return View(transactionType);
          }
        }
        else {
          ModelState.AddModelError(string.Empty, "Podatke o vrsti nije moguće povezati s forme");
          return View(transactionType);
        }
      }
      catch (Exception exc) {
        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        TempData[Constants.ErrorOccurred] = true;
        return RedirectToAction(nameof(Edit), Id);
      }
    }
  }
}
