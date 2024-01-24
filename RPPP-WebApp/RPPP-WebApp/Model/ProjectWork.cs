using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

/// <summary>
/// Represents a project work/activity entity.
/// </summary>
public partial class ProjectWork
{
    /// <summary>
    /// Gets or sets the unique identifier for the project work/activity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the work/activity assignee.
    /// </summary>
    public Guid AssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the title of the project work/activity.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the project work/activity.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the work/activity project.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the assignee navigation property associated with the project work/activity.
    /// </summary>
    public virtual Worker Assignee { get; set; }

    /// <summary>
    /// Gets or sets the labor diary entries associated with the project work/activity.
    /// </summary>
    public virtual ICollection<LaborDiary> LaborDiary { get; set; } = new List<LaborDiary>();

    /// <summary>
    /// Gets or sets the project navigation property associated with the project work/activity.
    /// </summary>
    public virtual Project Project { get; set; }

    /// <summary>
    /// Gets or sets the requirement task navigation property associated with the project work/activity.
    /// </summary>
    public virtual RequirementTask RequirementTask { get; set; }
}
