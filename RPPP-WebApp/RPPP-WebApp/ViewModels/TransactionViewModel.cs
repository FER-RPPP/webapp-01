namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a view model for displaying transactions.
  /// </summary>
  public class TransactionViewModel {

    /// <summary>
    /// Gets or sets the unique identifier of the transaction.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the International Bank Account Number (IBAN) associated with the transaction.
    /// </summary>
    public string Iban { get; set; }

    /// <summary>
    /// Gets or sets the recipient of the transaction.
    /// </summary>
    public string Recipient { get; set; }

    /// <summary>
    /// Gets or sets the amount of the transaction.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the date of the transaction.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the type of the transaction.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the purpose of the transaction.
    /// </summary>
    public string Purpose { get; set; }

  }
}
