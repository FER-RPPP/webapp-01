using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

/// <summary>
/// Represents a transaction in the RPPP01 application.
/// </summary>
public partial class Transaction
{
  /// <summary>
  /// Gets or sets the unique identifier for the transaction.
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  /// Gets or sets the amount of the transaction.
  /// </summary>
  public decimal Amount { get; set; }

  /// <summary>
  /// Gets or sets the recipient of the transaction.
  /// </summary>
  public string Recipient { get; set; }

  /// <summary>
  /// Gets or sets the date of the transaction.
  /// </summary>
  public DateTime Date { get; set; }

  /// <summary>
  /// Gets or sets the unique identifier for the transaction type.
  /// </summary>
  public Guid TypeId { get; set; }

  /// <summary>
  /// Gets or sets the unique identifier for the transaction purpose.
  /// </summary>
  public Guid PurposeId { get; set; }

  /// <summary>
  /// Gets or sets the IBAN associated with the transaction.
  /// </summary>
  public string Iban { get; set; }

  /// <summary>
  /// Gets or sets the navigation property for the associated project card (IBAN).
  /// </summary>
  public virtual ProjectCard IbanNavigation { get; set; }

  /// <summary>
  /// Gets or sets the navigation property for the associated transaction purpose.
  /// </summary>

  public virtual TransactionPurpose Purpose { get; set; }

  /// <summary>
  /// Gets or sets the navigation property for the associated transaction type.
  /// </summary>
  public virtual TransactionType Type { get; set; }
}
