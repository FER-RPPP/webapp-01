using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class ProjectRole
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<ProjectPartner> ProjectPartner { get; set; } = new List<ProjectPartner>();
}
