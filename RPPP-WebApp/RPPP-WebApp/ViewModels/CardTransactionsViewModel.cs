namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a view model for card transactions.
  /// </summary>
  public class CardTransactionsViewModel {
    /// <summary>
    /// Gets or sets the unique identifier of the transaction.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the IBAN (International Bank Account Number) associated with the transaction.
    /// </summary>
    public string Iban { get; set; }
    /// <summary>
    /// Gets or sets the OIB (Personal Identification Number) associated with the transaction.
    /// </summary>
    public string Oib { get; set; }
    /// <summary>
    /// Gets or sets the balance associated with the transaction.
    /// </summary>
    public decimal Balance { get; set; }
    /// <summary>
    /// Gets or sets the activation date of the card.
    /// </summary>
    public DateTime ActivationDate { get; set; }
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
    /// <summary>
    /// Gets or sets the owner of the card associated with the transaction.
    /// </summary>
    public string Owner { get; set; }
  }
}
