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

    /// <summary>
    /// Web API controller for handling CRUD operations on Product entities.
    /// </summary>

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

        /// <summary>
        /// Gets the count of Product entities optionally filtered by a query.
        /// </summary>
        /// <param name="filter">Query string to filter the Product entities.</param>
        /// <returns>The count of filtered or all Product entities.</returns>
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
        /// Retrieves a list of Product entities based on provided load parameters.
        /// </summary>
        /// <param name="loadParams">Parameters for loading data such as filtering, sorting, paging, etc.</param>
        /// <returns>A list of Product entities.</returns>
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
        /// Retrieves a specific Product entity by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the Product entity.</param>
        /// <returns>The Product entity if found, otherwise a 404 Not Found status.</returns>
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
        /// Deletes a specific Product entity by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the Product entity to delete.</param>
        /// <returns>A 204 No Content status if successful, otherwise a 404 Not Found status.</returns>
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

        /// <summary>
        /// Updates a specific Product entity identified by the provided id with the data in the provided model.
        /// </summary>
        /// <param name="id">The unique identifier of the Product entity to update.</param>
        /// <param name="model">The model containing updated data for the Product entity.</param>
        /// <returns>A 204 No Content status if successful, otherwise a 404 Not Found or 400 Bad Request status.</returns>
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
        /// Creates a new Product entity based on the provided model.
        /// </summary>
        /// <param name="model">The model to create a new Product entity from.</param>
        /// <returns>A 201 Created status with the newly created Product entity, otherwise a 400 Bad Request status.</returns>
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
