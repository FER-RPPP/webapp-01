using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a view model for displaying transaction purposes with paging information.
  /// </summary>
  public class TransactionPurposeViewModel{
    /// <summary>
    /// Gets or sets the collection of transaction purposes.
    /// </summary>
    public IEnumerable<TransactionPurpose> TransactionPurpose { get; set; }
    /// <summary>
    /// Gets or sets the paging information for the transaction purposes.
    /// </summary>
    public PagingInfo PagingInfo { get; set; }
  }
}