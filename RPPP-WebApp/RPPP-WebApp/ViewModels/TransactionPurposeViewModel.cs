using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  public class TransactionPurposeViewModel{
    public IEnumerable<TransactionPurpose> TransactionPurpose { get; set; }
    public PagingInfo PagingInfo { get; set; }
  }
}