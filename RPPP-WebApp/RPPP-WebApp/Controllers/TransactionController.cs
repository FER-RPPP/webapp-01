using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Extensions;

namespace RPPP_WebApp.Controllers {
  /// <summary>
  /// Controller for managing transactions.
  /// </summary>
  public class TransactionController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<TransactionController> logger;
    private readonly AppSettings appData;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionController"/> class.
    /// </summary>
    /// <param name="ctx">The database context.</param>
    /// <param name="options">Application settings.</param>
    /// <param name="logger">Logger instance.</param>
    public TransactionController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<TransactionController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

    /// <summary>
    /// Displays a paginated list of transactions based on filter criteria.
    /// </summary>
    /// <param name="filter">Filter criteria.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>The view displaying the paginated list of transactions.</returns>
    public async Task<IActionResult> Index(TransactionFilter filter, int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.Transaction
                     .AsNoTracking();

      string errorMessage = "Ne postoji niti jedna transakcija";
      if (!string.IsNullOrEmpty(filter.PurposeName)) {
        query = query.Where(p => p.Purpose.PurposeName.Contains(filter.PurposeName));
        errorMessage += $" gdje je svrha: {filter.PurposeName}";
      }

      if (!string.IsNullOrEmpty(filter.TypeName)) {
        query = query.Where(p => p.Type.TypeName.Contains(filter.TypeName));
        errorMessage += $" gdje je vrsta: {filter.TypeName}";
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

      var transaction = await query
                  .Select(o => new TransactionViewModel {
                    Id = o.Id,
                    Iban = o.IbanNavigation.Iban,
                    Recipient = o.Recipient,
                    Amount = o.Amount,
                    Date = o.Date,
                    Type = o.Type.TypeName,
                    Purpose = o.Purpose.PurposeName,
                  })
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToListAsync();

      var model = new TransactionsViewModel {
        Transaction = transaction,
        PagingInfo = pagingInfo,
        Filter = filter
      };

      return View(model);
    }

    /// <summary>
    /// Displays the form for creating a new transaction.
    /// </summary>
    /// <returns>The view for creating a new transaction.</returns>
    [HttpGet]
    public async Task<IActionResult> Create() {
      await PrepareDropDownLists();
      return View();
    }

    /// <summary>
    /// Handles the submission of the new transaction form.
    /// </summary>
    /// <param name="transaction">The transaction data from the form.</param>
    /// <returns>Redirects to the transaction index on success; returns the form on failure.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Transaction transaction) {
      if (ModelState.IsValid) {
        try {
          ctx.Add(transaction);
          await ctx.SaveChangesAsync();

          logger.LogInformation($"Transakcija {transaction.Id} dodana.");
          TempData[Constants.Message] = $"Transakcija je dodana.";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index));

        }
        catch (Exception exc) {
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          await PrepareDropDownLists();
          return View(transaction);
        }
      }
      else {
        await PrepareDropDownLists();
        return View(transaction);
      }
    }

    /// <summary>
    /// Deletes a transaction based on its ID.
    /// </summary>
    /// <param name="Id">The ID of the transaction to be deleted.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>Redirects to the transaction index after deletion.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid Id, int page = 1, int sort = 1, bool ascending = true) {
      var transaction = ctx.Transaction.Find(Id);
      if (transaction != null) {
        try {
          ctx.Remove(transaction);
          ctx.SaveChanges();
          logger.LogInformation($"Transakcija {Id} uspješno obrisana.");
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

    /// <summary>
    /// Displays the form for editing an existing transaction.
    /// </summary>
    /// <param name="id">The ID of the transaction to be edited.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>The view for editing an existing transaction.</returns>
    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      var transaction = ctx.Transaction.AsNoTracking().Where(o => o.Id == id).SingleOrDefault();
      if (transaction == null) {
        logger.LogWarning("Ne postoji transakcija: " + id);
        return NotFound("Ne postoji transakcija: " + id);
      }
      else {
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        await PrepareDropDownLists();
        return View(transaction);
      }
    }

    /// <summary>
    /// Handles the submission of the edited transaction form.
    /// </summary>
    /// <param name="id">The ID of the transaction to be updated.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>Redirects to the transaction index on success; returns the form on failure.</returns>
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      try {
        Transaction transaction = await ctx.Transaction
                          .Where(o => o.Id == id)
                          .FirstOrDefaultAsync();
        if (transaction == null) {
          return NotFound("Neispravan id transakcije: " + id);
        }

        if (await TryUpdateModelAsync(transaction, "",
            o => o.Id, o => o.Iban, o => o.Recipient, o => o.Amount, o => o.Date, o => o.TypeId, o => o.PurposeId
        )) {
          ViewBag.Page = page;
          ViewBag.Sort = sort;
          ViewBag.Ascending = ascending;
          try {
            await ctx.SaveChangesAsync();
            logger.LogInformation($"Transakcija {id} ažurirana.");
            TempData[Constants.Message] = $"Transakcija je ažurirana.";
            TempData[Constants.ErrorOccurred] = false;
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
          }
          catch (Exception exc) {
            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
            await PrepareDropDownLists();
            return View(transaction);
          }
        }
        else {
          ModelState.AddModelError(string.Empty, "Podatke o transakciji nije moguće povezati s forme");
          await PrepareDropDownLists();
          return View(transaction);
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
      var ibans = await ctx.ProjectCard
                           .ToListAsync();

      var types = await ctx.TransactionType
                          .ToListAsync();

      var purposes = await ctx.TransactionPurpose
                          .ToListAsync();

      var ibanList = ibans.Select(iban => new SelectListItem {
        Text = $"{iban.Iban}",
        Value = iban.Iban.ToString()
      }).ToList();

      var typeList = types.Select(type => new SelectListItem {
        Text = $"{type.TypeName}",
        Value = type.Id.ToString()
      }).ToList();

      var purposeList = purposes.Select(purpose => new SelectListItem {
        Text = $"{purpose.PurposeName}",
        Value = purpose.Id.ToString()
      }).ToList();

      ViewBag.Ibans = ibanList;
      ViewBag.Types = typeList;
      ViewBag.Purposes = purposeList;
    }
    
  }
}

