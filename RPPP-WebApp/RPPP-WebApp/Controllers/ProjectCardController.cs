using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Extensions;
using System.Text.Json;
using System.Diagnostics;

namespace RPPP_WebApp.Controllers {
  /// <summary>
  /// Controller for managing project cards.
  /// </summary>
  public class ProjectCardController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<ProjectCardController> logger;
    private readonly AppSettings appData;

    /// <summary>
    /// Constructor for the ProjectCardController.
    /// </summary>
    /// <param name="ctx">The database context.</param>
    /// <param name="options">Snapshot of application settings.</param>
    /// <param name="logger">Logger for logging messages.</param>
    public ProjectCardController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjectCardController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

    /// <summary>
    /// Displays the index page for project cards.
    /// </summary>
    /// <param name="filter">Filter parameters.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>Index view.</returns>
    public async Task<IActionResult> Index(ProjectCardFilter filter, int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.ProjectCard
                     .AsNoTracking();

      string errorMessage = "Ne postoji niti jedna projektna kartica";
      if (!string.IsNullOrEmpty(filter.Oib)) {
        query = query.Where(p => p.OibNavigation.Oib.Contains(filter.Oib));
        errorMessage += $" gdje je OIB vlasnika: {filter.Oib}";
      }

      if (!string.IsNullOrEmpty(filter.Name)) {
        query = query.Where(p => p.OibNavigation.Name.Contains(filter.Name));
        errorMessage += $" gdje je ime vlasnika: {filter.Name}";
      }

      if (!string.IsNullOrEmpty(filter.Surname)) {
        query = query.Where(p => p.OibNavigation.Surname.Contains(filter.Surname));
        errorMessage += $" gdje je prezime vlasnika: {filter.Surname}";
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

      var projectCard = await query
                  .Select(o => new ProjectCardViewModel {
                    Iban = o.Iban,
                    Balance = o.Balance,
                    ActivationDate = o.ActivationDate,
                    Owner = $"{o.OibNavigation.Name} {o.OibNavigation.Surname} ({o.OibNavigation.Oib})",
                    Recipient = MakeShorter(string.Join(", ", o.Transaction.Select(t => t.Recipient)), maxLength)
                  })
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToListAsync();

      var model = new ProjectCardsViewModel {
        ProjectCard = projectCard,
        PagingInfo = pagingInfo,
        Filter = filter,
      };

      return View(model);
    }

    /// <summary>
    /// Truncates a string to the specified maximum length.
    /// </summary>
    /// <param name="value">The input string to be truncated.</param>
    /// <param name="maxLength">The maximum length of the truncated string.</param>
    /// <returns>Truncated string with an ellipsis if needed.</returns>
    public static string MakeShorter(string value, int maxLength) {
      if (value.Length <= maxLength)
        return value;
      else
        return value.Substring(0, maxLength) + "...";
    }

    /// <summary>
    /// Displays the details of a project card including its transactions.
    /// </summary>
    /// <param name="id">The IBAN of the project card.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sorting option.</param>
    /// <param name="ascending">Sort order.</param>
    /// <returns>Details view.</returns>
    public async Task<IActionResult> Show(string id, int page = 1, int sort = 1, bool ascending = true) {
      await PrepareDropDownLists();

      var query = ctx.Transaction
                     .AsNoTracking();
      query = query.ApplySort(sort, ascending);

      var transaction = await query
                  .Where(o => o.Iban == id)
                  .Select(o => new TransactionViewModel {
                    Id = o.Id,
                    Recipient = o.Recipient,
                    Amount = o.Amount,
                    Date = o.Date,
                    Type = o.Type.TypeName,
                    Purpose = o.Purpose.PurposeName,
                  })
                  .ToListAsync();

      var projectCard = await ctx.ProjectCard
        .Where(o => o.Iban == id)
        .Select(o => new ProjectCardViewModel {
          Iban = o.Iban,
          Owner = o.OibNavigation.Name + " " + o.OibNavigation.Surname + " (" + o.Oib + ")",
          Balance = o.Balance,
          ActivationDate = o.ActivationDate
        })
        .FirstOrDefaultAsync();

      var model = new ProjectCardTransactionsViewModel {
        Transaction = transaction,
        ProjectCard = projectCard
      };

      return View(model);
    }

    /// <summary>
    /// Displays the form for creating a new project card.
    /// </summary>
    /// <returns>Create view.</returns>
    [HttpGet]
    public async Task<IActionResult> Create() {
      await PrepareDropDownLists();
      return View();
    }

    /// <summary>
    /// Handles the HTTP POST request for creating a new project card.
    /// </summary>
    /// <param name="projectCard">The project card data to be created.</param>
    /// <returns>Redirects to the index view if successful, otherwise returns the create view with error messages.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectCard projectCard) {
      if (ModelState.IsValid) {
        try {
          ctx.Add(projectCard);
          await ctx.SaveChangesAsync();
          logger.LogInformation($"Projektna kartica {projectCard.Iban} dodana.");
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

    /// <summary>
    /// Deletes a project card with the specified IBAN.
    /// </summary>
    /// <param name="Iban">The IBAN of the project card to be deleted.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">Whether the sorting is in ascending order.</param>
    /// <returns>Redirects to the index view with updated data.</returns>
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

    /// <summary>
    /// Displays the form for editing an existing project card.
    /// </summary>
    /// <param name="id">The IBAN of the project card to be edited.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">Whether the sorting is in ascending order.</param>
    /// <returns>Edit view if successful, otherwise returns a not found response.</returns>
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

    /// <summary>
    /// Handles the HTTP POST request for updating an existing project card.
    /// </summary>
    /// <param name="id">The IBAN of the project card to be updated.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">Whether the sorting is in ascending order.</param>
    /// <returns>Redirects to the index view if successful, otherwise returns the edit view with error messages.</returns>
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string id, int page = 1, int sort = 1, bool ascending = true) {
      try {
        ProjectCard projectCard = await ctx.ProjectCard
                          .Where(o => o.Iban == id)
                          .FirstOrDefaultAsync();
        if (projectCard == null) {
          logger.LogWarning($"Neispravan IBAN projektne kartice: {id})");
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
            logger.LogInformation($"Projektna kartica (IBAN = {id}) ažurirana.");
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

      var types = await ctx.TransactionType
                          .ToListAsync();

      var purposes = await ctx.TransactionPurpose
                          .ToListAsync();

      var ownersList = owners.Select(owner => new SelectListItem {
        Text = $"{owner.Name} {owner.Surname} ({owner.Oib})",
        Value = owner.Oib.ToString()
      }).ToList();

      var typeList = types.Select(type => new SelectListItem {
        Text = $"{type.TypeName}",
        Value = type.Id.ToString()
      }).ToList();

      var purposeList = purposes.Select(purpose => new SelectListItem {
        Text = $"{purpose.PurposeName}",
        Value = purpose.Id.ToString()
      }).ToList();

      ViewBag.Types = typeList;
      ViewBag.Purposes = purposeList;
      ViewBag.Owners = ownersList;
    }

    /// <summary>
    /// Gets details of a project card with the specified IBAN.
    /// </summary>
    /// <param name="id">The IBAN of the project card.</param>
    /// <returns>Partial view with details of the project card.</returns>
    [HttpGet]
    public async Task<IActionResult> Get(string id) {
      var projectCard = await ctx.ProjectCard
                                 .Where(o => o.Iban == id)
                                 .Select(o => new ProjectCardViewModel {
                                   Iban = o.Iban,
                                   Balance = o.Balance,
                                   ActivationDate = o.ActivationDate,
                                   Owner = o.OibNavigation.Name + " " + o.OibNavigation.Surname + " (" + o.Oib + ")",
                                 })
                                 .FirstOrDefaultAsync();
      if (projectCard != null) {
        return PartialView(projectCard);
      }
      else {
        return NotFound($"Neispravan Iban projektne kartice: {id}");
      }
    }

    /// <summary>
    /// Displays the form for editing a project card.
    /// </summary>
    /// <param name="id">The IBAN of the project card to edit.</param>
    /// <returns>Edit view with the form for editing the project card.</returns>
    [HttpGet]
    public async Task<IActionResult> Editt(string id) {
      var projectCard = await ctx.ProjectCard
                                 .Where(o => o.Iban == id)
                                 .Select(o => new ProjectCardViewModel {
                                   Iban = o.Iban,
                                   Balance = o.Balance,
                                   ActivationDate = o.ActivationDate,
                                   Owner = o.OibNavigation.Name + " " + o.OibNavigation.Surname + " (" + o.Oib + ")",
                                 })
                                 .FirstOrDefaultAsync();
      await PrepareDropDownLists();

      if (projectCard != null) {
        return PartialView(projectCard);
      }
      else {
        return NotFound($"Neispravan Iban projektne kartice: {id}");
      }
    }

    /// <summary>
    /// Handles the HTTP POST request for editing a project card.
    /// </summary>
    /// <param name="projectCard">The view model containing the edited project card data.</param>
    /// <returns>Redirects to the project card details view if successful, otherwise returns the edit view with error messages.</returns>
    [HttpPost]
    public async Task<IActionResult> Editt(ProjectCardViewModel projectCard) {
      if (projectCard == null) {
        return NotFound("Nema poslanih podataka");
      }
      ProjectCard dbProjectCard = await ctx.ProjectCard.FindAsync(projectCard.Iban);
      if (dbProjectCard == null) {
        return NotFound($"Neispravan Iban projektne kartice: {projectCard.Iban}");
      }

      if (ModelState.IsValid) {
        try {
          dbProjectCard.Iban = projectCard.Iban;
          dbProjectCard.Balance = projectCard.Balance;
          dbProjectCard.ActivationDate = projectCard.ActivationDate;

          if (projectCard.Owner.Contains("(") || projectCard.Owner.Contains(")")) {
            dbProjectCard.Oib = projectCard.Owner.Split("(")[1].Split(")")[0];
          }
          else {
            dbProjectCard.Oib = projectCard.Owner;
          }
          logger.LogInformation($"Projektna kartica (IBAN = {projectCard.Iban} ažurirana.");
          await ctx.SaveChangesAsync();
          return RedirectToAction(nameof(Show), new { id = projectCard.Iban });
        }
        catch (Exception exc) {
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          return PartialView(projectCard);
        }
      }
      else {
        return PartialView(projectCard);
      }
    }

    /// <summary>
    /// Gets details of a transaction with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the transaction.</param>
    /// <returns>Partial view with details of the transaction.</returns>
    [HttpGet]
    public async Task<IActionResult> GetTransaction(Guid id) {
      var transaction = await ctx.Transaction
                                 .Where(o => o.Id == id)
                                 .Select(o => new TransactionViewModel {
                                   Id = o.Id,
                                   Recipient = o.Recipient,
                                   Amount = o.Amount,
                                   Date = o.Date,
                                   Type = o.Type.TypeName,
                                   Purpose = o.Purpose.PurposeName,

                                 })
                                 .FirstOrDefaultAsync();

      if (transaction != null) {
        return PartialView(transaction);
      }
      else {
        return NotFound($"Neispravan Id transakcije: {id}");
      }
    }

    /// <summary>
    /// Displays the form for editing an existing transaction.
    /// </summary>
    /// <param name="id">The ID of the transaction to be edited.</param>
    /// <returns>EditTransaction partial view if successful, otherwise returns a not found response.</returns>
    [HttpGet]
    public async Task<IActionResult> EditTransaction(Guid id) {
      var transaction = await ctx.Transaction
                                 .Where(o => o.Id == id)
                                 .Select(o => new TransactionViewModel {
                                   Id = o.Id,
                                   Recipient = o.Recipient,
                                   Amount = o.Amount,
                                   Date = o.Date,
                                   Type = o.Type.TypeName,
                                   Purpose = o.Purpose.PurposeName,

                                 })
                                 .FirstOrDefaultAsync();

      await PrepareDropDownLists();
      if (transaction != null) {
        return PartialView(transaction);
      }
      else {
        return NotFound($"Neispravan Id transakcije: {id}");
      }
    }

    /// <summary>
    /// Handles the HTTP POST request for updating an existing transaction.
    /// </summary>
    /// <param name="transaction">The updated transaction data.</param>
    /// <returns>Redirects to the transaction details view if successful, otherwise returns the edit transaction partial view with error messages.</returns>
    [HttpPost]
    public async Task<IActionResult> EditTransaction(TransactionViewModel transaction) {
      if (transaction == null) {
        return NotFound("Nema poslanih podataka");
      }
      Transaction dbTransaction = await ctx.Transaction.FindAsync(transaction.Id);
      if (dbTransaction == null) {
        return NotFound($"Neispravan Id transakcije: {transaction.Id}");
      }

      if (ModelState.IsValid) {
        try {
          dbTransaction.Id = transaction.Id;
          dbTransaction.Recipient = transaction.Recipient;
          dbTransaction.Amount = transaction.Amount;
          dbTransaction.Date = transaction.Date;
          Guid typeId = new Guid(transaction.Type);
          dbTransaction.TypeId = typeId;
          Guid purposeId = new Guid(transaction.Purpose);
          dbTransaction.PurposeId = purposeId;

          logger.LogInformation($"Transakcija (id = {transaction.Id} ažurirana.");
          await ctx.SaveChangesAsync();
          return RedirectToAction(nameof(GetTransaction), new { id = transaction.Id });
        }
        catch (Exception exc) {
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          return PartialView(transaction);
        }
      }
      else {
        return PartialView(transaction);
      }
    }

    /// <summary>
    /// Handles the HTTP Delete for deleting a transaction.
    /// </summary>
    /// <param name="id">The ID of the transaction to be deleted.</param>
    /// <returns>DeleteTransaction partial view if successful, otherwise returns a not found response.</returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteTransaction(Guid Id) {
      ActionResponseMessage responseMessage;

      var transaction = ctx.Transaction.AsNoTracking().Where(o => o.Id == Id).SingleOrDefault();
      if (transaction != null) {
        try {
          ctx.Remove(transaction);
          await ctx.SaveChangesAsync();
          logger.LogInformation($"Transakcija s id = {Id} uspješno obrisana.");
          responseMessage = new ActionResponseMessage(MessageType.Success, $"Transakcija uspješno obrisana.");
        }
        catch (Exception exc) {
          logger.LogError($"Pogreška prilikom brisanja transakcije.");
          responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja transakcije: {exc.CompleteExceptionMessage()}");
        }
      }
      else {
        logger.LogError($"Transakcija s Id-om {Id} ne postoji");
        responseMessage = new ActionResponseMessage(MessageType.Error, $"Transakcija s Id-om {Id} ne postoji");
      }

      Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
      return responseMessage.MessageType == MessageType.Success ?
       new EmptyResult() : await GetTransaction(Id);

    }

    /// <summary>
    /// Displays the form for adding a new transaction to a project card.
    /// </summary>
    /// <returns>AddTransaction partial view with the form for adding a new transaction.</returns>
    [HttpGet]
    public async Task<IActionResult> AddTransaction() {
      await PrepareDropDownLists();
      return PartialView();
    }

    /// <summary>
    /// Handles the HTTP POST request for adding a new transaction to a project card.
    /// </summary>
    /// <param name="transactionVM">The view model containing the new transaction data.</param>
    /// <returns>Redirects to the project card details view if successful, otherwise returns the add transaction partial view with error messages.</returns>
    [HttpPost]
    public async Task<IActionResult> AddTransaction(ProjectCardTransactionsViewModel transactionVM) {
      if (!ModelState.IsValid) {
        await PrepareDropDownLists();
        return PartialView(transactionVM);
      }
      Guid typeId = new Guid("1E17C459-1C24-46A1-92F1-0767EA793AD1");
      Guid purposeId = new Guid("3F4D8891-B9CD-4F41-AAFD-34DAEAE88121");

      var newTransaction = new Transaction {
        Iban = transactionVM.NewTransaction.Iban,
        Recipient = transactionVM.NewTransaction.Recipient,
        Amount = transactionVM.NewTransaction.Amount,
        Date = transactionVM.NewTransaction.Date,
        TypeId = new Guid("1E17C459-1C24-46A1-92F1-0767EA793AD1"),
        PurposeId = new Guid("3F4D8891-B9CD-4F41-AAFD-34DAEAE88121")
      };

      try {
        ctx.Add(newTransaction);
        logger.LogInformation($"Transakcija s id = {transactionVM.NewTransaction.Id} uspješno dodana.");
        await ctx.SaveChangesAsync();
        TempData["Message"] = "Transakcija je dodana.";
        TempData["ErrorOccurred"] = false;
        return RedirectToAction(nameof(Show));
      }
      catch (Exception exc) {
        ModelState.AddModelError(string.Empty, exc.Message);
        await PrepareDropDownLists();
        return PartialView(transactionVM);
      }

    }

    /// <summary>
    /// Displays the form for adding a new transaction.
    /// </summary>
    /// <returns>NewTransaction partial view with the form for adding a new transaction.</returns>
    [HttpGet]
    public async Task<IActionResult> NewTransaction() {
      await PrepareDropDownLists();
      return PartialView();
    }

    /// <summary>
    /// Handles the HTTP POST request for adding a new transaction.
    /// </summary>
    /// <param name="transaction">The new transaction data.</param>
    /// <returns>Redirects to the project card details view if successful, otherwise returns the new transaction partial view with error messages.</returns>
    [HttpPost]
    public async Task<IActionResult> NewTransaction(Transaction transaction) {
      ActionResponseMessage responseMessage;
      if (ModelState.IsValid) {
        try {
          Debug.WriteLine($"{transaction.Iban} {transaction.Recipient} {transaction.Amount} {transaction.Date} {transaction.TypeId} {transaction.PurposeId})");
          ctx.Add(transaction);
          await ctx.SaveChangesAsync();
          logger.LogInformation($"Transakcija {transaction.Id} dodana.");
          responseMessage = new ActionResponseMessage(MessageType.Success, $"Transakcija {transaction.Iban} dodana.");
         // return RedirectToAction(nameof(Show));

        }
        catch (Exception exc) {
          logger.LogError($"Pogreška prilikom dodavanja transakcije {transaction.Id}.");
          responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom dodavanja transakcije {transaction.Iban}.");
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

          await PrepareDropDownLists();
          //return PartialView(transaction);
        }
      }
      else {
        logger.LogError($"Pogreška prilikom dodavanja transakcije {transaction.Id}.");
        responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom dodavanja transakcije {transaction.Iban}.");

        await PrepareDropDownLists();
       // return PartialView(transaction);
      }

      Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
      return responseMessage.MessageType == MessageType.Success ?
       Content($"<script>setTimeout(function() {{ window.location.href='/rppp/01/ProjectCard/Show/{transaction.Iban}'; }}, 1000);</script>", "text/html") : PartialView(transaction);
    }
  }
}