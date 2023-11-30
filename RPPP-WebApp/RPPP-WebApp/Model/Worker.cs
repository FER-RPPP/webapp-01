using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Worker
{
    public Guid Id { get; set; }

    public string Email { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public Guid OrganizationId { get; set; }

    public string PhoneNumber { get; set; }

    public virtual ICollection<LaborDiary> LaborDiary { get; set; } = new List<LaborDiary>();

    public virtual Organization Organization { get; set; }

    public virtual ICollection<ProjectPartner> ProjectPartner { get; set; } = new List<ProjectPartner>();

    public virtual ICollection<ProjectWork> ProjectWork { get; set; } = new List<ProjectWork>();
}
