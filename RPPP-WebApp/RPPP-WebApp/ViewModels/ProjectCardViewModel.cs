namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a view model for displaying details of a project card.
  /// </summary>
  public class ProjectCardViewModel {
    /// <summary>
    /// Gets or sets the International Bank Account Number (IBAN) of the project card.
    /// </summary>
    public string Iban { get; set; }

    /// <summary>
    /// Gets or sets the balance of the project card.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the activation date of the project card.
    /// </summary>
    public DateTime ActivationDate { get; set; }

    /// <summary>
    /// Gets or sets the owner of the project card.
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// Gets or sets the recipient associated with the project card.
    /// </summary>
    public string Recipient { get; set; }
  }
}
