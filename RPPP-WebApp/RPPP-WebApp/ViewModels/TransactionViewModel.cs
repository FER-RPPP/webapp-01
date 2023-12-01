namespace RPPP_WebApp.ViewModels {
  public class TransactionViewModel {

    public Guid Id { get; set; }

    public string Iban { get; set; }

    public string Recipient { get; set; }

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public string Type { get; set; }

    public string Purpose { get; set; }

  }
}
