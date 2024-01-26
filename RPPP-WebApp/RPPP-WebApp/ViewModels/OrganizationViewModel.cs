using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{ /// <summary>
  /// Represents a view model for displaying organizations with paging information.
  /// </summary>
    public class OrganizationViewModel
    { /// <summary>
      /// Gets or sets the collection of organizations.
      /// </summary>
        public IEnumerable<Organization> Organizations { get; set; }

        /// <summary>
        /// Gets or sets the paging information for organizations.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
    }
}
