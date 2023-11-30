using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class TransactionPurpose
{
    public Guid Id { get; set; }

    public string PurposeName { get; set; }

    public virtual ICollection<Transaction> Transaction { get; set; } = new List<Transaction>();
}
