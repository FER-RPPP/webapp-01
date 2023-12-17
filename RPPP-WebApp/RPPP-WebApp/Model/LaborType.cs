using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class LaborType
{
    public Guid Id { get; set; }

    public string Type { get; set; }

    public virtual ICollection<LaborDiary> LaborDiary { get; set; } = new List<LaborDiary>();
}
