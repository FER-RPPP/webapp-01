using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{

    /// <summary>
    /// Controller for handling JTable operations for products.
    /// Inherits from the generic JTableController, with ProductController as the specific controller type.
    /// </summary>
    [Route("jtable/product/[action]")]
    public class ProductJTableController : JTableController<ProductController, Guid, ProductViewModel>
    {
        public ProductJTableController(ProductController controller) : base(controller)
        {

        }

        /// <summary>
        /// Updates a product with the specified model data.
        /// </summary>
        /// <param name="model">The ProductViewModel containing the data for the update.</param>
        /// <returns>A JTableAjaxResult indicating the result of the update operation.</returns>
        [HttpPost]
        public async Task<JTableAjaxResult> Update([FromForm] ProductViewModel model)
        {
            return await base.UpdateItem(model.Id, model);
        }

        /// <summary>
        /// Deletes a product identified by the specified id.
        /// </summary>
        /// <param name="id">The unique identifier of the product to delete.</param>
        /// <returns>A JTableAjaxResult indicating the result of the delete operation.</returns>
        [HttpPost]
        public async Task<JTableAjaxResult> Delete([FromForm] Guid id)
        {
            return await base.DeleteItem(id);
        }
    }
}
