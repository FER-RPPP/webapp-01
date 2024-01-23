namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents paging information for a collection of items.
  /// </summary>
  public class PagingInfo {
    /// <summary>
    /// Gets or sets the total number of items.
    /// </summary>
    public int TotalItems { get; set; }
    /// <summary>
    /// Gets or sets the number of items displayed per page.
    /// </summary>
    public int ItemsPerPage { get; set; }
    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int CurrentPage { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the sort order is ascending.
    /// </summary>
    public bool Ascending { get; set; }
    /// <summary>
    /// Gets the total number of pages based on the total items and items per page.
    /// </summary>
    public int TotalPages {
      get {
        return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
      }
    }
    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    public int Sort { get; set; }
  }
}
