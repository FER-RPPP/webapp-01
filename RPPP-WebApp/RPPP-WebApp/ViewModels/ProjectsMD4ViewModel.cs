namespace RPPP_WebApp.ViewModels {
    /// <summary>
    /// Represents a view model for displaying a list of projects (domain 4 version).
    /// </summary>
    public class ProjectsMD4ViewModel {

    /// <summary>
    /// Gets or sets the collection of project (domain 4 version) view models.
    /// </summary>
    public IEnumerable<ProjectMD4ViewModel> ProjectMD4 { get; set; }
    /// <summary>
    /// Gets or sets the paging information for the list of projects.
    /// </summary>
    public PagingInfo PagingInfo { get; set; }
  }
}
