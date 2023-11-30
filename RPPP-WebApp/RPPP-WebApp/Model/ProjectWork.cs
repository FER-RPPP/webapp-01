using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class ProjectWork
{
    public Guid Id { get; set; }

    public Guid AssigneeId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public Guid ProjectId { get; set; }

    public virtual Worker Assignee { get; set; }

    public virtual ICollection<LaborDiary> LaborDiary { get; set; } = new List<LaborDiary>();

    public virtual Project Project { get; set; }

    public virtual RequirementTask RequirementTask { get; set; }
}
