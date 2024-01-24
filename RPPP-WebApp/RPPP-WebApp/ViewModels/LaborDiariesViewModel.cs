namespace RPPP_WebApp.ViewModels {
    /// <summary>
    /// Represents a view model for displaying labor diary entries with paging information and optional filtering.
    /// </summary>
    public class LaborDiariesViewModel {

    /// <summary>
    /// Gets or sets the collection of labor diary entries.
    /// </summary>
    public IEnumerable<LaborDiaryViewModel> LaborDiary { get; set; }
    /// <summary>
    /// Gets or sets the paging information for the labor diary entries.
    /// </summary>
    public PagingInfo PagingInfo { get; set; }
    /// <summary>
    /// Gets or sets the filter criteria for labor diary entries.
    /// </summary>
    public LaborDiaryFilter Filter { get; set; }
  }
}
