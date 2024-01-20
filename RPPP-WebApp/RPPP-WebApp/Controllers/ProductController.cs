using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.Services;
using RPPP_WebApp.ViewModels;
using System.Linq.Expressions;

namespace RPPP_WebApp.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase, ICustomController<Guid, ProductViewModel>
    {

        private readonly Rppp01Context ctx;
        private static Dictionary<string, Expression<Func<Product, object>>> orderSelectors = new()
        {
            [nameof(ProductViewModel.Id).ToLower()] = m => m.Id,
            [nameof(ProductViewModel.Name).ToLower()] = m => m.Name
        };

        private static Expression<Func<Product, ProductViewModel>> projection = m => new ProductViewModel
        {
            Id = m.Id,
            Name = m.Name
        };

        public ProductController(Rppp01Context ctx)
        {
            this.ctx = ctx;
        }


        // <summary>
        /// Vraća broj svih produkta filtriran prema nazivu mjesta 
        /// </summary>
        /// <param name="filter">Opcionalni filter za naziv produkta</param>
        /// <returns></returns>
        [HttpGet("count", Name = "BrojProdukta")]
        public async Task<int> Count([FromQuery] string filter)
        {
            var query = ctx.Product.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(m => m.Name.Contains(filter));
            }
            int count = await query.CountAsync();
            return count;
        }


        /// <summary>
        /// Dohvat produkta (opcionalno filtrirano po nazivu produkta).
        /// Broj produkta, poredak, početna pozicija određeni s loadParams.
        /// </summary>
        /// <param name="loadParams">Postavke za straničenje i filter</param>
        /// <returns></returns>
        [HttpGet(Name = "DohvatiMjesta")]
        public async Task<List<ProductViewModel>> GetAll([FromQuery] LoadParams loadParams)
        {
            var query = ctx.Product.AsQueryable();

            if (!string.IsNullOrWhiteSpace(loadParams.Filter))
            {
                query = query.Where(m => m.Name.Contains(loadParams.Filter));
            }

            if (loadParams.SortColumn != null)
            {
                if (orderSelectors.TryGetValue(loadParams.SortColumn.ToLower(), out var expr))
                {
                    query = loadParams.Descending ? query.OrderByDescending(expr) : query.OrderBy(expr);
                }
            }

            var list = await query.Select(projection)
                                  .Skip(loadParams.StartIndex)
                                  .Take(loadParams.Rows)
                                  .ToListAsync();
            return list;
        }

        /// <summary>
        /// Vraća produkt čiji je id jednak vrijednosti parametra id
        /// </summary>
        /// <param name="id">IdProdukta</param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "DohvatiProdukt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductViewModel>> Get(Guid id)
        {
            var mjesto = await ctx.Product
                                  .Where(m => m.Id == id)
                                  .Select(projection)
                                  .FirstOrDefaultAsync();
            if (mjesto == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
            }
            else
            {
                return mjesto;
            }
        }

        /// <summary>
        /// Brisanje produkta određenog s id
        /// </summary>
        /// <param name="id">Vrijednost primarnog ključa (Id produkta)</param>
        /// <returns></returns>
        /// <response code="204">Ako je produkt uspješno obrisan</response>
        /// <response code="404">Ako produkt s poslanim id-om ne postoji</response>      
        [HttpDelete("{id}", Name = "ObrisiProdukt")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var mjesto = await ctx.Product.FindAsync(id);
            if (mjesto == null)
            {
                return NotFound();
            }
            else
            {
                ctx.Remove(mjesto);
                await ctx.SaveChangesAsync();
                return NoContent();
            };
        }

        // <summary>
        /// Ažurira produkt
        /// </summary>
        /// <param name="id">parametar čija vrijednost jednoznačno identificira produkt</param>
        /// <param name="model">Podaci o produktu. Id mora se podudarati s parametrom id</param>
        /// <returns></returns>
        [HttpPut("{id}", Name = "AzurirajMjesto")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, ProductViewModel model)
        {
            if (model.Id != id) //ModelState.IsValid i model != null provjera se automatski zbog [ApiController]
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.Id}");
            }
            else
            {
                var mjesto = await ctx.Product.FindAsync(id);
                if (mjesto == null)
                {
                    return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
                }

                mjesto.Name = model.Name;
              
                await ctx.SaveChangesAsync();
                return NoContent();
            }
        }

        /// <summary>
        /// Stvara novi produkt opisanim poslanim modelom
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost(Name = "DodajProdukt")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            Product product = new Product
            {
                Id = model.Id,
                Name = model.Name
            };
            ctx.Add(product);
            await ctx.SaveChangesAsync();

            var addedItem = await Get(product.Id);

            return CreatedAtAction(nameof(Get), new { id = product.Id }, addedItem.Value);
        }

    }
}
