using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Client
{
    public Guid Id { get; set; }

    public string Oib { get; set; }

    public string Iban { get; set; }

    public string Email { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public virtual ICollection<Project> Project { get; set; } = new List<Project>();
}
