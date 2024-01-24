namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Represents a view model for displaying workers.
    /// </summary>
    public class WorkerViewModel
    {    /// <summary>
         /// Gets or sets the unique identifier of the worker.
         /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Gets or sets the email address of the worker.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the first name of the worker.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Gets or sets the last name of the worker.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Gets or sets the phone number of the worker.
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Gets or sets the unique identifier of the organization the worker belongs to.
        /// </summary>
        public string Organization { get; set; }

    }
}
