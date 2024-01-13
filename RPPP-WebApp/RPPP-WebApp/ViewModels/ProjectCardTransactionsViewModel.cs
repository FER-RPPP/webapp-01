using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  public class ProjectCardTransactionsViewModel {
    public IEnumerable<TransactionViewModel> Transaction { get; set; }
    public Transaction NewTransaction { get; set; }
    public PagingInfo PagingInfo { get; set; }
    public TransactionFilter Filter { get; set; }
    public ProjectCardViewModel ProjectCard { get; set; }
  }
}
