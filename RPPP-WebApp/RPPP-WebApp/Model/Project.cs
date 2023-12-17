using RPPP_WebApp.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class Project
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Please select a client.")]
    public Guid ClientId { get; set; }

    [Required(ErrorMessage = "Name cannot be empty")]
    public string Name { get; set; }

    [Required]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Type must contain only letters.")]
    public string Type { get; set; }

    [Required(ErrorMessage = "Please select an Owner.")]
    public string OwnerId { get; set; }

    [Required(ErrorMessage = "Please select a Card.")]
    public string CardId { get; set; }

    public virtual ProjectCard Card { get; set; }

    public virtual Client Client { get; set; }

    public virtual ICollection<Document> Document { get; set; } = new List<Document>();

    public virtual Owner Owner { get; set; }

    public virtual ICollection<ProjectPartner> ProjectPartner { get; set; } = new List<ProjectPartner>();

    public virtual ICollection<ProjectRequirement> ProjectRequirement { get; set; } = new List<ProjectRequirement>();

    public virtual ICollection<ProjectWork> ProjectWork { get; set; } = new List<ProjectWork>();
}
