using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;
/// <summary>
/// Represents a project role in the RPPP01 application.
/// </summary>
public partial class ProjectRole
{/// <summary>
 /// Gets or sets the unique identifier of the role.
 /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the collection of ProjectPartner entities associated with this role.
    /// </summary>
    public virtual ICollection<ProjectPartner> ProjectPartner { get; set; } = new List<ProjectPartner>();
}
