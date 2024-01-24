using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

/// <summary>
/// Represents a labor diary entry entity.
/// </summary>
public partial class LaborDiary
{
    /// <summary>
    /// Gets or sets the unique identifier for the labor diary entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the labor diary entry work.
    /// </summary>
    public Guid WorkId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the labor diary entry worker.
    /// </summary>
    public Guid WorkerId { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the labor diary entry.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the hours spent on the labor diary entry.
    /// </summary>
    public decimal HoursSpent { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the labor type.
    /// </summary>
    public Guid LaborTypeId { get; set; }

    /// <summary>
    /// Gets or sets the description of the labor diary entry.
    /// </summary>
    public string LaborDescription { get; set; }

    /// <summary>
    /// Gets or sets the labor type navigation property associated with the labor diary entry.
    /// </summary>
    public virtual LaborType LaborType { get; set; }

    /// <summary>
    /// Gets or sets the work navigation property associated with the labor diary entry.
    /// </summary>
    public virtual ProjectWork Work { get; set; }

    /// <summary>
    /// Gets or sets the worker navigation property associated with the labor diary entry.
    /// </summary>
    public virtual Worker Worker { get; set; }
}
