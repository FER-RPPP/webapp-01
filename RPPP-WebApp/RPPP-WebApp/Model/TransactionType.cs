using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class TransactionType
{
    public Guid Id { get; set; }

    public string TypeName { get; set; }

    public virtual ICollection<Transaction> Transaction { get; set; } = new List<Transaction>();
}
