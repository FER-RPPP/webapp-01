using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers {
	public class OwnerController : Controller {
    /// <summary>
    /// Controller for handling actions related to owners.
    /// </summary>
		private readonly Rppp01Context ctx;
    private readonly ILogger<OwnerController> logger;
    private readonly AppSettings appData;

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnerController"/> class.
    /// </summary>
    /// <param name="ctx">The database context.</param>
    /// <param name="options">The application settings options.</param>
    /// <param name="logger">The logger.</param>
    public OwnerController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<OwnerController> logger) {
			this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
		}

    /// <summary>
    /// Displays a paginated list of owners.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="sort">The sort option.</param>
    /// <param name="ascending">The sort direction.</param>
    /// <returns>The result of the action.</returns>
    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.Owner
                     .AsNoTracking();

      int count = query.Count();
      if (count == 0) {
        logger.LogInformation("Ne postoji niti jedan vlasnik.");
        TempData[Constants.Message] = "Ne postoji niti jedan vlasnik.";
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

      var owner = query
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToList();

      var model = new OwnerViewModel {
        Owner = owner,
        PagingInfo = pagingInfo
      };

      return View(model);
		}

    /// <summary>
    /// Displays the view for creating a new owner.
    /// </summary>
    /// <returns>The result of the action.</returns>
    [HttpGet]
    public IActionResult Create() {
      return View();
    }

    /// <summary>
    /// Handles the HTTP POST request for creating a new owner.
    /// </summary>
    /// <param name="owner">The owner to be created.</param>
    /// <returns>The result of the action.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Owner owner) {
      logger.LogTrace(JsonSerializer.Serialize(owner));
      if (ModelState.IsValid) {
        try {
          ctx.Add(owner);
          ctx.SaveChanges();
          logger.LogInformation(new EventId(1000), $" Vlasnik {owner.Name} {owner.Surname} je dodan.");

          TempData[Constants.Message] = $"Vlasnik {owner.Name} {owner.Surname} je dodan.";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index));
        }
        catch (Exception exc) {
          logger.LogError("Pogreška prilikom dodavanja novog vlasnika: {0}", exc.CompleteExceptionMessage());
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          return View(owner);
        }
      }
      else {
        return View(owner);
      }
    }

    /// <summary>
    /// Handles the HTTP POST request for deleting an owner.
    /// </summary>
    /// <param name="Oib">The OIB of the owner to be deleted.</param>
    /// <param name="page">The page number.</param>
    /// <param name="sort">The sort option.</param>
    /// <param name="ascending">The sort direction.</param>
    /// <returns>The result of the action.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(string Oib, int page = 1, int sort = 1, bool ascending = true) {
      var owner = ctx.Owner.Find(Oib);
      if (owner != null) {
        try {
          ctx.Remove(owner);
          ctx.SaveChanges();
          logger.LogInformation($"Vlasnik {owner.Name} {owner.Surname} uspješno obrisan.");
          TempData[Constants.Message] = $"Vlasnik {owner.Name} {owner.Surname} uspješno obrisan.";
          TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc) {
          TempData[Constants.Message] = "Pogreška prilikom brisanja vlasnika: " + exc.CompleteExceptionMessage();
          TempData[Constants.ErrorOccurred] = true;
          logger.LogError("Pogreška prilikom brisanja vlasnika: " + exc.CompleteExceptionMessage());
        }
      }
      else {
        logger.LogWarning("Ne postoji vlasnik s OIB-om: {0} ", Oib);
        TempData[Constants.Message] = "Ne postoji vlasnik s OIB-om: " + Oib;
        TempData[Constants.ErrorOccurred] = true;
      }
      return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
    }

    /// <summary>
    /// Displays the view for editing an existing owner.
    /// </summary>
    /// <param name="id">The OIB of the owner to be edited.</param>
    /// <param name="page">The page number.</param>
    /// <param name="sort">The sort option.</param>
    /// <param name="ascending">The sort direction.</param>
    /// <returns>The result of the action.</returns>
    [HttpGet]
    public IActionResult Edit(string id, int page = 1, int sort = 1, bool ascending = true) {
      var owner = ctx.Owner.AsNoTracking().Where(o => o.Oib == id).SingleOrDefault();
      if (owner == null) {
        logger.LogWarning($"Ne postoji vlasnik s OIB-om: {id}");
        return NotFound("Ne postoji vlasnik s OIB-om: " + id);
      }
      else {
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        return View(owner);
      }
    }

    /// <summary>
    /// Handles the HTTP POST request for updating an existing owner.
    /// </summary>
    /// <param name="id">The OIB of the owner to be updated.</param>
    /// <param name="page">The page number.</param>
    /// <param name="sort">The sort option.</param>
    /// <param name="ascending">The sort direction.</param>
    /// <returns>The result of the action.</returns>
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string id, int page = 1, int sort = 1, bool ascending = true) {
      try {
        Owner owner = await ctx.Owner
                          .Where(o => o.Oib == id)
                          .FirstOrDefaultAsync();
        if (owner == null) {
          logger.LogWarning($"Neispravan OIB vlasnika: {id}");
          return NotFound("Neispravan OIB vlasnika: " + id);
        }

        if (await TryUpdateModelAsync<Owner>(owner, "",
            o => o.Oib, o => o.Name, o => o.Surname
        )) {
          ViewBag.Page = page;
          ViewBag.Sort = sort;
          ViewBag.Ascending = ascending;
          try {
            await ctx.SaveChangesAsync();
            logger.LogInformation($"Vlasnik (OIB = {id}) ažuriran.");
            TempData[Constants.Message] = $"Vlasnik (OIB = {id}) ažuriran.";
            TempData[Constants.ErrorOccurred] = false;
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
          }
          catch (Exception exc) {
            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
            return View(owner);
          }
        }
        else {
          ModelState.AddModelError(string.Empty, "Podatke o vlasniku nije moguće povezati s forme");
          return View(owner);
        }
      }
      catch (Exception exc) {
        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        TempData[Constants.ErrorOccurred] = true;
        return RedirectToAction(nameof(Edit), id);
      }
    }
  }
}




