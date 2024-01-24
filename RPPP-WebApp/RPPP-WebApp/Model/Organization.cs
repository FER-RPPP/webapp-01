using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;
/// <summary>
/// Represents an Organization in the RPPP01 application.
/// </summary>
public partial class Organization
{/// <summary>
 /// Gets or sets the unique identifier of the role.
 /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the name of the organization.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the collection of Worker entities associated with this organization.
    /// </summary>
    public virtual ICollection<Worker> Worker { get; set; } = new List<Worker>();
}
