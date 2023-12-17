using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class OrganizationViewModel
    {
        public IEnumerable<Organization> Organizations { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
