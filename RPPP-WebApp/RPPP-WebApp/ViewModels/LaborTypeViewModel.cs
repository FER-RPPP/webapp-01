using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
    /// <summary>
    /// Represents a view model for displaying labor types with paging information.
    /// </summary>
    public class LaborTypeViewModel {
        /// <summary>
        /// Gets or sets the collection of labor types.
        /// </summary>
        public IEnumerable<LaborType> LaborType { get; set; }
        /// <summary>
        /// Gets or sets the paging information for the labor types.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
  }
}
