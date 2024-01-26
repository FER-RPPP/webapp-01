using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

/// <summary>
/// Represents a purpose of transactions.
/// </summary>
public partial class TransactionPurpose
{
  /// <summary>
  /// Gets or sets the unique identifier for the transaction purpose.
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  /// Gets or sets the name of the transaction purpose.
  /// </summary>
  public string PurposeName { get; set; }

  /// <summary>
  /// Gets or sets the collection of transactions associated with this purpose.
  /// </summary>
  public virtual ICollection<Transaction> Transaction { get; set; } = new List<Transaction>();
}
