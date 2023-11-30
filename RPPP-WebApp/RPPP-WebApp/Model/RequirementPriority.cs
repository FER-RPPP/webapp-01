using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class RequirementPriority
{
    public Guid Id { get; set; }

    public string Type { get; set; }

    public virtual ICollection<ProjectRequirement> ProjectRequirement { get; set; } = new List<ProjectRequirement>();
}
