using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers {
  public class TransactionPurposeController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<TransactionPurposeController> logger;
    private readonly AppSettings appData;

    public TransactionPurposeController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<TransactionPurposeController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.TransactionPurpose
                     .AsNoTracking();

      int count = query.Count();
      if (count == 0) {
        logger.LogInformation("Ne postoji niti jedna svrha transakcije.");
        TempData[Constants.Message] = "Ne postoji niti jedna svrha transakcije.";
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

      var purpose = query
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToList();

      var model = new TransactionPurposeViewModel {
        TransactionPurpose = purpose,
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
    public IActionResult Create(TransactionPurpose transactionPurpose) {
      logger.LogTrace(JsonSerializer.Serialize(transactionPurpose));
      if (ModelState.IsValid) {
        try {
          ctx.Add(transactionPurpose);
          ctx.SaveChanges();
          logger.LogInformation(new EventId(1000), $"Svrha {transactionPurpose.PurposeName} je dodana.");

          TempData[Constants.Message] = $"Svrha {transactionPurpose.PurposeName} je dodana";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index));
        }
        catch (Exception exc) {
          logger.LogError("Pogreška prilikom dodavanja nove svrhe: {0}", exc.CompleteExceptionMessage());
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          return View(transactionPurpose);
        }
      }
      else {
        return View(transactionPurpose);
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid Id, int page = 1, int sort = 1, bool ascending = true) {
      var transactionPurpose = ctx.TransactionPurpose.Find(Id);
      if (transactionPurpose != null) {
        try {
          ctx.Remove(transactionPurpose);
          ctx.SaveChanges();
          logger.LogInformation($"Svrha {transactionPurpose.PurposeName} uspješno obrisana.");
          TempData[Constants.Message] = $"Svrha {transactionPurpose.PurposeName} uspješno obrisana.";
          TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc) {
          TempData[Constants.Message] = "Pogreška prilikom brisanja svrhe: " + exc.CompleteExceptionMessage();
          TempData[Constants.ErrorOccurred] = true;
          logger.LogError("Pogreška prilikom brisanja svrhe: " + exc.CompleteExceptionMessage());
        }
      }
      else {
        logger.LogWarning("Ne postoji svrha");
        TempData[Constants.Message] = "Ne postoji svrha";
        TempData[Constants.ErrorOccurred] = true;
      }
      return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
    }

    [HttpGet]
    public IActionResult Edit(Guid Id, int page = 1, int sort = 1, bool ascending = true) {
      var transactionPurpose = ctx.TransactionPurpose.AsNoTracking().Where(o => o.Id == Id).SingleOrDefault();
      if (transactionPurpose == null) {
        logger.LogWarning("Ne postoji ta svrha");
        return NotFound("Ne postoji ta svrha");
      }
      else {
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        return View(transactionPurpose);
      }
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid Id, int page = 1, int sort = 1, bool ascending = true) {
      try {
        TransactionPurpose transactionPurpose = await ctx.TransactionPurpose
                          .Where(o => o.Id == Id)
                          .FirstOrDefaultAsync();
        if (transactionPurpose == null) {
          return NotFound("Neispravan id svrhe");
        }

        if (await TryUpdateModelAsync<TransactionPurpose>(transactionPurpose, "",
            o => o.PurposeName, o => o.Id
        )) {
          ViewBag.Page = page;
          ViewBag.Sort = sort;
          ViewBag.Ascending = ascending;
          try {
            await ctx.SaveChangesAsync();
            TempData[Constants.Message] = $"Svrha {transactionPurpose.PurposeName} ažurirana.";
            TempData[Constants.ErrorOccurred] = false;
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
          }
          catch (Exception exc) {
            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
            return View(transactionPurpose);
          }
        }
        else {
          ModelState.AddModelError(string.Empty, "Podatke o svrsi nije moguće povezati s forme");
          return View(transactionPurpose);
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
