namespace RPPP_WebApp.ViewModels {
  public class ProjectWorksViewModel {

    public IEnumerable<ProjectWorkViewModel> ProjectWork { get; set; }
    public PagingInfo PagingInfo { get; set; }
    public ProjectWorkFilter Filter { get; set; }
  }
}
