using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class ProjectCard
{
    public string Iban { get; set; }

    public decimal Balance { get; set; }

    public DateTime ActivationDate { get; set; }

    public string Oib { get; set; }

    public virtual Owner OibNavigation { get; set; }

    public virtual ICollection<Project> Project { get; set; } = new List<Project>();

    public virtual ICollection<Transaction> Transaction { get; set; } = new List<Transaction>();
}
