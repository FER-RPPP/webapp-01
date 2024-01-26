using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

/// <summary>
/// Represents a type of transactions.
/// </summary>
public partial class TransactionType
{
  /// <summary>
  /// Gets or sets the unique identifier for the transaction type.
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  /// Gets or sets the name of the transaction type.
  /// </summary>
  public string TypeName { get; set; }

  /// <summary>
  /// Gets or sets the collection of transactions associated with this type.
  /// </summary>
  public virtual ICollection<Transaction> Transaction { get; set; } = new List<Transaction>();
}
