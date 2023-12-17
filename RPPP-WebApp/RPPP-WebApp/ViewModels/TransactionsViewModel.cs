namespace RPPP_WebApp.ViewModels {
  public class TransactionsViewModel {

    public IEnumerable<TransactionViewModel> Transaction { get; set; }
    public PagingInfo PagingInfo { get; set; }
    public TransactionFilter Filter { get; set; }
  }
}
