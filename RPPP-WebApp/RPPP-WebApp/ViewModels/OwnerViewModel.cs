using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  public class OwnerViewModel {
    public IEnumerable<Owner> Owner {  get; set; }
    public PagingInfo PagingInfo { get; set; }
  }
}
