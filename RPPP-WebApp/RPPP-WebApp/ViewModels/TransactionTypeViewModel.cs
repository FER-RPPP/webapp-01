using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  public class TransactionTypeViewModel {
    public IEnumerable<TransactionType> TransactionType { get; set; }
    public PagingInfo PagingInfo { get; set; }
  }
}
