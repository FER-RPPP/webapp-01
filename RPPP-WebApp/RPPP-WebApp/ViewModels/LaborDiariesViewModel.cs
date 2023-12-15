namespace RPPP_WebApp.ViewModels {
  public class LaborDiariesViewModel {

    public IEnumerable<LaborDiaryViewModel> LaborDiary { get; set; }
    public PagingInfo PagingInfo { get; set; }
    public LaborDiaryFilter Filter { get; set; }
  }
}
