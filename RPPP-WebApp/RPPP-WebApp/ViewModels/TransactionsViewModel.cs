namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a view model for displaying transactions with paging information and optional filtering.
  /// </summary>
  public class TransactionsViewModel {

    /// <summary>
    /// Gets or sets the collection of transactions.
    /// </summary>
    public IEnumerable<TransactionViewModel> Transaction { get; set; }
    /// <summary>
    /// Gets or sets the paging information for the transactions.
    /// </summary>
    public PagingInfo PagingInfo { get; set; }
    /// <summary>
    /// Gets or sets the filter criteria for transactions.
    /// </summary>
    public TransactionFilter Filter { get; set; }
  }
}
