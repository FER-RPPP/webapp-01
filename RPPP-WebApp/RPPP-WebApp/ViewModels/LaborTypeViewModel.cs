using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  public class LaborTypeViewModel {
    public IEnumerable<LaborType> LaborType { get; set; }
    public PagingInfo PagingInfo { get; set; }
  }
}
