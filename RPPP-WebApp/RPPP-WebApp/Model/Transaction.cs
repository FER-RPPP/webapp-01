using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Transaction
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public string Recipient { get; set; }

    public DateTime Date { get; set; }

    public Guid TypeId { get; set; }

    public Guid PurposeId { get; set; }

    public string Iban { get; set; }

    public virtual ProjectCard IbanNavigation { get; set; }

    public virtual TransactionPurpose Purpose { get; set; }

    public virtual TransactionType Type { get; set; }
}
