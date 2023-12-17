using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Model;

public partial class ProjectRequirement
{
    public Guid Id { get; set; }

    [Required]
    [RegularExpression(@"^[a-zA-Z]{1,20}$", ErrorMessage = "Type may only include alphabetic characters and may not be longer than 20 characters!")]

    public string Type { get; set; }

    public Guid RequirementPriorityId { get; set; }

    public Guid ProjectId { get; set; }

    public string Description { get; set; }

    public virtual Project Project { get; set; }

    public virtual RequirementPriority RequirementPriority { get; set; }

    public virtual ICollection<RequirementTask> RequirementTask { get; set; } = new List<RequirementTask>();
}
