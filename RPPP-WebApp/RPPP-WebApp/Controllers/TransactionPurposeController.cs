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
  /// <summary>
  /// Controller for managing transaction purposes.
  /// </summary>
  public class TransactionPurposeController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<TransactionPurposeController> logger;
    private readonly AppSettings appData;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionPurposeController"/> class.
    /// </summary>
    /// <param name="ctx">The database context.</param>
    /// <param name="options">Application settings.</param>
    /// <param name="logger">Logger instance.</param>
    public TransactionPurposeController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<TransactionPurposeController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

    /// <summary>
    /// Displays a paginated list of transaction purposes.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>The view displaying the paginated list of transaction purposes.</returns>
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

    /// <summary>
    /// Displays the form for creating a new transaction purpose.
    /// </summary>
    /// <returns>The view for creating a new transaction purpose.</returns>
    [HttpGet]
    public IActionResult Create() {
      return View();
    }

    /// <summary>
    /// Handles the submission of the new transaction purpose form.
    /// </summary>
    /// <param name="transactionPurpose">The transaction purpose data from the form.</param>
    /// <returns>Redirects to the transaction purpose index on success; returns the form on failure.</returns>
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

    /// <summary>
    /// Deletes a transaction purpose based on its ID.
    /// </summary>
    /// <param name="Id">The ID of the transaction purpose to be deleted.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>Redirects to the transaction purpose index after deletion.</returns>
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

    /// <summary>
    /// Displays the form for editing an existing transaction purpose.
    /// </summary>
    /// <param name="Id">The ID of the transaction purpose to be edited.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>The view for editing an existing transaction purpose.</returns>
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

    /// <summary>
    /// Handles the submission of the edited transaction purpose form.
    /// </summary>
    /// <param name="Id">The ID of the transaction purpose to be updated.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>Redirects to the transaction purpose index on success; returns the form on failure.</returns>
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
            logger.LogInformation($"Svrha {transactionPurpose.PurposeName} ažurirana.");
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
