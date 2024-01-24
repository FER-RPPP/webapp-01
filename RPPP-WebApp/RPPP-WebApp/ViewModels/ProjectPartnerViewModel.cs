namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Represents a view model for displaying partnerships.
    /// </summary>
    public class ProjectPartnerViewModel
    {/// <summary>
     /// Gets or sets the unique identifier of the partnership.
     /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the Project entity associated with this partnership.
        /// </summary>
        public string Project { get; set; }
        /// <summary>
        /// Gets or sets the name of the worker associated with this partnership.
        /// </summary>
        public string Worker { get; set; }
        /// <summary>
        /// Gets or sets the name of the role associated with this partnership.
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// Gets or sets the start date of the partnership.
        /// </summary>
        public DateTime DateFrom { get; set; }
        /// <summary>
        /// Gets or sets the end date of the partnership.
        /// </summary>
        public DateTime? DateTo { get; set; }
      
    }
}