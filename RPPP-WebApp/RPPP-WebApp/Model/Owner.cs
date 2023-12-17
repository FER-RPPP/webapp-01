using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class Owner
{
  [Display(Name = "OIB", Prompt = "Unesite OIB")]
  public string Oib { get; set; }

  [Display(Name = "Ime", Prompt = "Unesite ime")]
  public string Name { get; set; }

  [Display(Name = "Prezime", Prompt = "Unesite prezime")]
  public string Surname { get; set; }

  public virtual ICollection<Project> Project { get; set; } = new List<Project>();

  public virtual ICollection<ProjectCard> ProjectCard { get; set; } = new List<ProjectCard>();
}
