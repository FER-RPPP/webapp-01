using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Owner
{
    public string Oib { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public virtual ICollection<Project> Project { get; set; } = new List<Project>();

    public virtual ICollection<ProjectCard> ProjectCard { get; set; } = new List<ProjectCard>();
}
