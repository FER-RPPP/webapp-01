using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class TaskStatus
{
    public Guid Id { get; set; }

    public string Type { get; set; }

    public virtual ICollection<RequirementTask> RequirementTask { get; set; } = new List<RequirementTask>();
}
