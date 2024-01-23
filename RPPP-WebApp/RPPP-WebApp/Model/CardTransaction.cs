namespace RPPP_WebApp.Model {
  /// <summary>
  /// Represents a card transaction entity.
  /// </summary>
  public class CardTransaction {

    /// <summary>
    /// Gets or sets the IBAN.
    /// </summary>
    public string Iban { get; set; }

    /// <summary>
    /// Gets or sets the balance.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the activation date.
    /// </summary>
    public DateTime ActivationDate { get; set; }

    /// <summary>
    /// Gets or sets the OIB (Personal Identification Number).
    /// </summary>
    public string Oib { get; set; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the recipient.
    /// </summary>
    public string Recipient { get; set; }

    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the type ID.
    /// </summary>
    public Guid TypeId { get; set; }

    /// <summary>
    /// Gets or sets the purpose ID.
    /// </summary>
    public Guid PurposeId { get; set; }

    /// <summary>
    /// Gets or sets the associated project card navigation property.
    /// </summary>
    public virtual ProjectCard IbanNavigation { get; set; }

    /// <summary>
    /// Gets or sets the associated transaction purpose navigation property.
    /// </summary>
    public virtual TransactionPurpose Purpose { get; set; }

    /// <summary>
    /// Gets or sets the associated transaction type navigation property.
    /// </summary>
    public virtual TransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets the associated owner navigation property.
    /// </summary>
    public virtual Owner OibNavigation { get; set; }

    /// <summary>
    /// Gets or sets the collection of associated projects.
    /// </summary>
    public virtual ICollection<Project> Project { get; set; } = new List<Project>();

    /// <summary>
    /// Gets or sets the collection of associated transactions.
    /// </summary>
    public virtual ICollection<Transaction> Transaction { get; set; } = new List<Transaction>();
  }
}
