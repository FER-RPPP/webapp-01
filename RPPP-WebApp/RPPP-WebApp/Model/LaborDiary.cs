using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class LaborDiary
{
    public Guid Id { get; set; }

    public Guid WorkId { get; set; }

    public Guid WorkerId { get; set; }

    public DateTime Date { get; set; }

    public decimal HoursSpent { get; set; }

    public Guid LaborTypeId { get; set; }

    public string LaborDescription { get; set; }

    public virtual LaborType LaborType { get; set; }

    public virtual ProjectWork Work { get; set; }

    public virtual Worker Worker { get; set; }
}
