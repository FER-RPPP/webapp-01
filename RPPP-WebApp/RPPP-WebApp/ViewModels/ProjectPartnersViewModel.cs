using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class ProjectPartnersViewModel
    {
        public IEnumerable<ProjectPartnerViewModel> Partners { get; set; }

        public PagingInfo PagingInfo { get; set; }

        public ProjectPartnerFilter Filter { get; set; }
    }
}
