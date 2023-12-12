namespace RPPP_WebApp.ViewModels {
  public class ProjectCardViewModel {
    public string Iban { get; set; }

    public decimal Balance { get; set; }

    public DateTime ActivationDate { get; set; }

    public string Owner { get; set; }

    public string Recipient { get; set; }
  }
}
