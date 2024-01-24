using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

/// <summary>
/// Represents a worker in the RPPP01 application.
/// </summary>
public partial class Worker
{  /// <summary>
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
    /// Gets or sets the unique identifier of the organization the worker belongs to.
    /// </summary>
    public Guid OrganizationId { get; set; }
    /// <summary>
    /// Gets or sets the phone number of the worker.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the collection of LaborDiary entities associated with this worker.
    /// </summary>
    public virtual ICollection<LaborDiary> LaborDiary { get; set; } = new List<LaborDiary>();
    /// <summary>
    /// Gets or sets the Organization entity associated with this worker.
    /// </summary>
    public virtual Organization Organization { get; set; }

    /// <summary>
    /// Gets or sets the collection of ProjectPartner entities associated with this worker.
    /// </summary>
    public virtual ICollection<ProjectPartner> ProjectPartner { get; set; } = new List<ProjectPartner>();

    /// <summary>
    /// Gets or sets the collection of ProjectWork entities associated with this workers.
    /// </summary>
    public virtual ICollection<ProjectWork> ProjectWork { get; set; } = new List<ProjectWork>();
}
