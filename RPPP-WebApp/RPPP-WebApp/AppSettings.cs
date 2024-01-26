namespace RPPP_WebApp {
  /// <summary>
  /// Represents the application settings.
  /// </summary>
  public class AppSettings {
    /// <summary>
    /// Gets or sets the page size for pagination.
    /// </summary>
    public int PageSize { get; set; } = 10;
    /// <summary>
    /// Gets or sets the page offset for pagination.
    /// </summary>
    public int PageOffset { get; set; } = 10;
    /// <summary>
    /// Gets or sets the count for auto-complete suggestions.
    /// </summary>
    public int AutoCompleteCount { get; set; } = 50;

  }
}
