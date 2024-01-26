using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

/// <summary>
/// Represents a type of labor.
/// </summary>
public partial class LaborType
{
    /// <summary>
    /// Gets or sets the unique identifier for the labor type.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the labor type.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the collection of labor diary entries associated with this type.
    /// </summary>
    public virtual ICollection<LaborDiary> LaborDiary { get; set; } = new List<LaborDiary>();
}
