namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a filter for transactions based on purpose name and type name.
  /// </summary>
  public class TransactionFilter {

    /// <summary>
    /// Gets or sets the purpose name used for filtering transactions.
    /// </summary>
    public string PurposeName { get; set; }
    /// <summary>
    /// Gets or sets the type name used for filtering transactions.
    /// </summary>
    public string TypeName { get; set; }

  }
}
