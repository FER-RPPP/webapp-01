using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

/// <summary>
/// Represents an owner entity.
/// </summary>
public partial class Owner
{
  /// <summary>
  /// Gets or sets the OIB (Personal Identification Number) of the owner.
  /// </summary>
  [Display(Name = "OIB", Prompt = "Unesite OIB")]
  public string Oib { get; set; }

  /// <summary>
  /// Gets or sets the name of the owner.
  /// </summary>
  [Display(Name = "Ime", Prompt = "Unesite ime")]
  public string Name { get; set; }

  /// <summary>
  /// Gets or sets the surname of the owner.
  /// </summary>
  [Display(Name = "Prezime", Prompt = "Unesite prezime")]
  public string Surname { get; set; }

  /// <summary>
  /// Gets or sets the projects associated with the owner.
  /// </summary>
  public virtual ICollection<Project> Project { get; set; } = new List<Project>();

  /// <summary>
  /// Gets or sets the project cards associated with the owner.
  /// </summary>
  public virtual ICollection<ProjectCard> ProjectCard { get; set; } = new List<ProjectCard>();
}
