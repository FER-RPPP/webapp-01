using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{  /// <summary>
   /// Represents a view model for displaying workers with paging information and optional filtering.
   /// </summary>
    public class WorkerViewsModel
    {    /// <summary>
         /// Gets or sets the collection of workers.
         /// </summary>
        public IEnumerable<WorkerViewModel> Workers { get; set; }
        /// <summary>
        /// Gets or sets the paging information for the workers.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
        /// <summary>
        /// Gets or sets the filter criteria for workers.
        /// </summary>
        public WorkerFilter Filter { get; set; }
    }
}
