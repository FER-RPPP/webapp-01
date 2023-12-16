using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class RequirementTask
{
    public Guid Id { get; set; }

    public DateTime PlannedStartDate { get; set; }

    public DateTime PlannedEndDate { get; set; }

    public DateTime? ActualStartDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public Guid TaskStatusId { get; set; }

    public Guid ProjectRequirementId { get; set; }

    public virtual ProjectWork ProjectWork{ get; set; }

    public virtual ProjectRequirement ProjectRequirement { get; set; }

    public virtual TaskStatus TaskStatus { get; set; }
}
