using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{ /// <summary>
  /// Represents a view model for displaying roles with paging information.
  /// </summary>
    public class ProjectRoleViewModel
    {/// <summary>
     /// Gets or sets the collection of roles.
     /// </summary>
        public IEnumerable<ProjectRole> Roles { get; set; }
        /// <summary>
        /// Gets or sets the paging information for the roles.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
    }
}
