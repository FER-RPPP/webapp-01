using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a view model for displaying transaction types with paging information.
  /// </summary>
  public class TransactionTypeViewModel {
    /// <summary>
    /// Gets or sets the collection of transaction types.
    /// </summary>
    public IEnumerable<TransactionType> TransactionType { get; set; }
    /// <summary>
    /// Gets or sets the paging information for the transaction types.
    /// </summary>
    public PagingInfo PagingInfo { get; set; }
  }
}
