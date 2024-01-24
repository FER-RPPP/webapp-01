using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace RPPP_WebApp.ViewModels {
    /// <summary>
    /// Represents a filter for searching project works/activites based on title, assignee, and project.
    /// </summary>
    public class ProjectWorkFilter {

    /// <summary>
    /// Gets or sets the title for filtering.
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// Gets or sets the assignee for filtering.
    /// </summary>
    public string Assignee { get; set; }
    /// <summary>
    /// Gets or sets the project for filtering.
    /// </summary>
    public string Project { get; set; }

  }
}
