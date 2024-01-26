using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
    /// <summary>
    /// Represents a view model for displaying projects (domain 4 version).
    /// </summary>
    public class ProjectMD4ViewModel {
        /// <summary>
        /// Gets or sets the unique identifier for the project.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the project.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the owner of the project.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets the client of the project.
        /// </summary>
        public string Client { get; set; }

        /// <summary>
        /// Gets or sets the International Bank Account Number (IBAN) of the project.
        /// </summary>
        public string Iban { get; set; }
    }
}
