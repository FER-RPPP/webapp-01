namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a view model for displaying a list of project cards.
  /// </summary>
  public class ProjectCardsViewModel {

    /// <summary>
    /// Gets or sets the collection of project card view models.
    /// </summary>
    public IEnumerable<ProjectCardViewModel> ProjectCard { get; set; }
    /// <summary>
    /// Gets or sets the paging information for the list of project cards.
    /// </summary>
    public PagingInfo PagingInfo { get; set; }
    /// <summary>
    /// Gets or sets the filter criteria for searching project cards.
    /// </summary>
    public ProjectCardFilter Filter { get; set; }
  }
}
