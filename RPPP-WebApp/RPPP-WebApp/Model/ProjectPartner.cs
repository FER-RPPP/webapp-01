using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class ProjectPartner
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Guid WorkerId { get; set; }

    public Guid RoleId { get; set; }

    public DateTime DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public virtual Project Project { get; set; }

    public virtual ProjectRole Role { get; set; }

    public virtual Worker Worker { get; set; }
}
