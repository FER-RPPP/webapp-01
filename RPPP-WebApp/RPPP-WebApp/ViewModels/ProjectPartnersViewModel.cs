using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{  /// <summary>
   /// Represents a view model for displaying partnerships with paging information and optional filtering.
   /// </summary>
    public class ProjectPartnersViewModel
    { /// <summary>
      /// Gets or sets the collection of partnerships.
      /// </summary>
        public IEnumerable<ProjectPartnerViewModel> Partners { get; set; }
        /// <summary>
        /// Gets or sets a new partnership.
        /// </summary>
        public ProjectPartner newPartner { get; set; }
        /// <summary>
        /// Gets or sets the paging information for the partnerships.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
        /// <summary>
        /// Gets or sets the filter criteria for partnerships.
        /// </summary>
        public ProjectPartnerFilter Filter { get; set; }
        /// <summary>
        /// Gets or sets the related Worker entity.
        /// </summary>
        public Worker worker { get; set; }

    }
}
