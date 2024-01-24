using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
    /// <summary>
    /// Represents a view model for displaying labor diary entries.
    /// </summary>
    public class LaborDiaryViewModel {
        /// <summary>
        /// Gets or sets the unique identifier for the labor diary entry.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the work of the labor diary entry.
        /// </summary>
        public string Work { get; set; }

        /// <summary>
        /// Gets or sets the worker of the labor diary entry.
        /// </summary>
        public string Worker { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the labor diary entry.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the hours spent for the labor diary entry.
        /// </summary>
        public decimal HoursSpent { get; set; }

        /// <summary>
        /// Gets or sets the labor type of the labor diary entry.
        /// </summary>
        public string LaborType { get; set; }

        /// <summary>
        /// Gets or sets the labor description of the labor diary entry.
        /// </summary>
        public string LaborDescription { get; set; }
    }
}
