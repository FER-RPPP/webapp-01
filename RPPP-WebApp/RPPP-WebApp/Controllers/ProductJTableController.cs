using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    [Route("jtable/product/[action]")]
    public class ProductJTableController : JTableController<ProductController, Guid, ProductViewModel>
    {
        public ProductJTableController(ProductController controller) : base(controller)
        {

        }

        [HttpPost]
        public async Task<JTableAjaxResult> Update([FromForm] ProductViewModel model)
        {
            return await base.UpdateItem(model.Id, model);
        }

        [HttpPost]
        public async Task<JTableAjaxResult> Delete([FromForm] Guid id)
        {
            return await base.DeleteItem(id);
        }
    }
}
