﻿using Microsoft.AspNetCore.Mvc;
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
  public class ProjectCardController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<ProjectCardController> logger;
    private readonly AppSettings appData;

    public ProjectCardController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjectCardController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

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

    public static string MakeShorter(string value, int maxLength) {
      if (value.Length <= maxLength)
        return value;
      else
        return value.Substring(0, maxLength) + "...";
    }


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

    [HttpDelete]
    public async Task<IActionResult> DeleteTransaction(Guid Id) {
      ActionResponseMessage responseMessage;

      var transaction = ctx.Transaction.AsNoTracking().Where(o => o.Id == Id).SingleOrDefault();
      if (transaction != null) {
        try {
          ctx.Remove(transaction);
          await ctx.SaveChangesAsync();
          responseMessage = new ActionResponseMessage(MessageType.Success, $"Transakcija uspješno obrisana.");
        }
        catch (Exception exc) {
          responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja transakcije: {exc.CompleteExceptionMessage()}");
        }
      }
      else {
        responseMessage = new ActionResponseMessage(MessageType.Error, $"Transakcija s Id-om {Id} ne postoji");
      }

      Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
      return responseMessage.MessageType == MessageType.Success ?
       new EmptyResult() : await GetTransaction(Id);

    }


    [HttpGet]
    public async Task<IActionResult> AddTransaction() {
      await PrepareDropDownLists();
      return PartialView();
    }

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


    [HttpGet]
    public async Task<IActionResult> NewTransaction() {
      await PrepareDropDownLists();
      return PartialView();
    }

    [HttpPost]
    public async Task<IActionResult> NewTransaction(Transaction transaction) {
      ActionResponseMessage responseMessage;
      if (ModelState.IsValid) {
        try {
          Debug.WriteLine($"{transaction.Iban} {transaction.Recipient} {transaction.Amount} {transaction.Date} {transaction.TypeId} {transaction.PurposeId})");
          ctx.Add(transaction);
          await ctx.SaveChangesAsync();

          responseMessage = new ActionResponseMessage(MessageType.Success, $"Transakcija {transaction.Iban} dodana.");
         // return RedirectToAction(nameof(Show));

        }
        catch (Exception exc) {
          responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom dodavanja transakcije {transaction.Iban}. {transaction.Recipient} {transaction.Amount} {transaction.Date} {transaction.TypeId} {transaction.PurposeId}");
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

          await PrepareDropDownLists();
          //return PartialView(transaction);
        }
      }
      else {
        responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom dodavanja transakcije {transaction.Iban}. {transaction.Recipient} {transaction.Amount} {transaction.Date} {transaction.TypeId} {transaction.PurposeId}");

        await PrepareDropDownLists();
       // return PartialView(transaction);
      }

      Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
      return responseMessage.MessageType == MessageType.Success ?
       Content($"<script>setTimeout(function() {{ window.location.href='/ProjectCard/Show/{transaction.Iban}'; }}, 1000);</script>", "text/html") : PartialView(transaction);
    }
  }
}