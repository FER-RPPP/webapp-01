using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

/// <summary>
/// Represents a project card entity.
/// </summary>
public partial class ProjectCard
{
  /// <summary>
  /// Gets or sets the IBAN (International Bank Account Number) of the project card.
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
  /// Gets or sets the OIB (Personal Identification Number) associated with the project card.
  /// </summary>
  public string Oib { get; set; }

  /// <summary>
  /// Gets or sets the owner navigation property associated with the project card.
  /// </summary>
  public virtual Owner OibNavigation { get; set; }

  /// <summary>
  /// Gets or sets the projects associated with the project card.
  /// </summary>
  public virtual ICollection<Project> Project { get; set; } = new List<Project>();

  /// <summary>
  /// Gets or sets the transactions associated with the project card.
  /// </summary>
  public virtual ICollection<Transaction> Transaction { get; set; } = new List<Transaction>();
}
