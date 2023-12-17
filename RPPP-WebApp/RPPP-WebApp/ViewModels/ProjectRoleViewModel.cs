using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class ProjectRoleViewModel
    {
        public IEnumerable<ProjectRole> Roles { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
