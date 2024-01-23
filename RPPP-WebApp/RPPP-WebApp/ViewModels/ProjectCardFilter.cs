using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a filter for searching project cards based on OIB, name, and surname.
  /// </summary>
  public class ProjectCardFilter {

    /// <summary>
    /// Gets or sets the OIB (Personal Identification Number) for filtering.
    /// </summary>
    public string Oib { get; set; }
    /// <summary>
    /// Gets or sets the name for filtering.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the surname for filtering.
    /// </summary>
    public string Surname { get; set; }

  }
}
