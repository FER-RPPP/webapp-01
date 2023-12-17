using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  public class ProjectWorkViewModel {
    public Guid Id { get; set; }

    public string Assignee { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string Project { get; set; }

    public string DiaryEntries { get; set; }
  }
}
