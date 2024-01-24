namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Represents a filter for partnerships based on projects, workers and roles.
    /// </summary>
    public class ProjectPartnerFilter
    {    /// <summary>
         /// Gets or sets the project name used for filtering partnerships.
         /// </summary>
        public string Project { get; set; }
        /// <summary>
        /// Gets or sets the worker name used for filtering partnerships.
        /// </summary>
        public string Worker { get; set; }
        /// <summary>
        /// Gets or sets the worker name used for filtering partnerships.
        /// </summary>
        public string Role { get; set; }
    }
}
