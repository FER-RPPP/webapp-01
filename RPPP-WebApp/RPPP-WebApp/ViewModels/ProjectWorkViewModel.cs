using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
    /// <summary>
    /// Represents a view model for displaying details of a project work/activity.
    /// </summary>
    public class ProjectWorkViewModel {
    /// <summary>
    /// Gets or sets the unique identifier for the project work/activity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the assignee of the project work/activity.
    /// </summary>
    public string Assignee { get; set; }

    /// <summary>
    /// Gets or sets the title of the project work/activity.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the project work/activity.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the project of the project work/activity.
    /// </summary>
    public string Project { get; set; }

    /// <summary>
    /// Gets or sets the labor diary entries of the project work/activity.
    /// </summary>
    public string DiaryEntries { get; set; }
  }
}
