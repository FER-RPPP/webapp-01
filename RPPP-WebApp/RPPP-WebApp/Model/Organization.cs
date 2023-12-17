using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Organization
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Worker> Worker { get; set; } = new List<Worker>();
}
