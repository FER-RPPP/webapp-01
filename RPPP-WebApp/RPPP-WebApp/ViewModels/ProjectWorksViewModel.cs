namespace RPPP_WebApp.ViewModels {
    /// <summary>
    /// Represents a view model for displaying a list of project works/activities.
    /// </summary>
    public class ProjectWorksViewModel {

    /// <summary>
    /// Gets or sets the collection of project work/activity view models.
    /// </summary>
    public IEnumerable<ProjectWorkViewModel> ProjectWork { get; set; }
    /// <summary>
    /// Gets or sets the paging information for the list of project works/activities.
    /// </summary>
    public PagingInfo PagingInfo { get; set; }
    /// <summary>
    /// Gets or sets the filter criteria for searching project works/activities.
    /// </summary>
    public ProjectWorkFilter Filter { get; set; }
  }
}
