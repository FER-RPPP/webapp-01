using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Project
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public string OwnerId { get; set; }

    public string CardId { get; set; }

    public virtual ProjectCard Card { get; set; }

    public virtual Client Client { get; set; }

    public virtual ICollection<Document> Document { get; set; } = new List<Document>();

    public virtual Owner Owner { get; set; }

    public virtual ICollection<ProjectPartner> ProjectPartner { get; set; } = new List<ProjectPartner>();

    public virtual ICollection<ProjectRequirement> ProjectRequirement { get; set; } = new List<ProjectRequirement>();

    public virtual ICollection<ProjectWork> ProjectWork { get; set; } = new List<ProjectWork>();
}
