using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Extensions;

namespace RPPP_WebApp.Controllers {
  public class TransactionController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<TransactionController> logger;
    private readonly AppSettings appData;

    public TransactionController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<TransactionController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

    public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.Transaction
                     .AsNoTracking();

      int count = await query.CountAsync();
      if (count == 0) {
        logger.LogInformation("Ne postoji niti jedna transakcija.");
        TempData[Constants.Message] = "Ne postoji niti jedna transakcija.";
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
                  .Select(m => new TransactionViewModel {
                    Iban = m.IbanNavigation.Iban,
                    Recipient = m.Recipient,
                    Amount = m.Amount,
                    Date = m.Date,
                    Type = m.Type.TypeName,
                    Purpose = m.Purpose.PurposeName,
                  })
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToListAsync();

      var model = new TransactionsViewModel {
        Transaction = transaction,
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
    public async Task<IActionResult> Create(Transaction transaction) {
      if (ModelState.IsValid) {
        try {
          ctx.Add(transaction);
          await ctx.SaveChangesAsync();

          TempData[Constants.Message] = $"Transakcija {transaction.Id} je dodana.";
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

