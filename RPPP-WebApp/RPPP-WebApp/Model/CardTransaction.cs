namespace RPPP_WebApp.Model {
  public class CardTransaction {

    public string Iban { get; set; }

    public decimal Balance { get; set; }

    public DateTime ActivationDate { get; set; }

    public string Oib { get; set; }

    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public string Recipient { get; set; }

    public DateTime Date { get; set; }

    public Guid TypeId { get; set; }

    public Guid PurposeId { get; set; }

    public virtual ProjectCard IbanNavigation { get; set; }

    public virtual TransactionPurpose Purpose { get; set; }

    public virtual TransactionType Type { get; set; }

    public virtual Owner OibNavigation { get; set; }

    public virtual ICollection<Project> Project { get; set; } = new List<Project>();

    public virtual ICollection<Transaction> Transaction { get; set; } = new List<Transaction>();
  }
}
