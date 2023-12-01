using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  public class ProjectCardsViewModel {
    public IEnumerable<ProjectCardViewModel> ProjectCard { get; set; }
    public PagingInfo PagingInfo { get; set; }
  }
}
