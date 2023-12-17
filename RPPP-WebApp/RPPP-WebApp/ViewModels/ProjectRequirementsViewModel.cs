using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class ProjectRequirementsViewModel
    {
        public IEnumerable<ProjectRequirement> ProjectRequirement { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
