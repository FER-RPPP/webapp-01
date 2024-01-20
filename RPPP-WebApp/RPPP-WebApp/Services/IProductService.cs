using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.Controllers;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Services
{
    public interface IProductService : ICustomController<Guid, Product>
    {
        
    }
}
