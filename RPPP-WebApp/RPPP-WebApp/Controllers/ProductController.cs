using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.Services;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : JTableController<IProductService, Guid, Product>
    {



        public ProductController(IProductService productService)
            : base(productService)
        {
           
        }




    }
}
