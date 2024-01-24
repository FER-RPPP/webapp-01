using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

/// <summary>
/// Represents a partnership in the RPPP01 application.
/// </summary>
public partial class ProjectPartner
{ /// <summary>
  /// Gets or sets the unique identifier of the partnership.
  /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier of the project the partnership belongs to.
    /// </summary>
    public Guid ProjectId { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier of the worker involved in the partnership.
    /// </summary>
    public Guid WorkerId { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier of the role assigned the worker.
    /// </summary>

    public Guid RoleId { get; set; }
    /// <summary>
    /// Gets or sets the start date of the partnership.
    /// </summary>
    public DateTime DateFrom { get; set; }
    /// <summary>
    /// Gets or sets the end date of the partnership.
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Gets or sets the Project entity associated with this partnership.
    /// </summary>
    public virtual Project Project { get; set; }

    /// <summary>
    /// Gets or sets the Project entity associated with this partnership.
    /// </summary>
    public virtual ProjectRole Role { get; set; }

    /// <summary>
    /// Gets or sets the Worker entity associated with this partnership.
    /// </summary>
    public virtual Worker Worker { get; set; }
}
