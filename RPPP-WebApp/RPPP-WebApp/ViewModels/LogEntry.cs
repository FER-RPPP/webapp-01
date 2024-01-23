namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a log entry with various properties such as time, ID, controller, level, message, URL, and action.
  /// </summary>
  public class LogEntry {
    /// <summary>
    /// Gets or sets the timestamp of the log entry.
    /// </summary>
    public DateTime Time { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the log entry.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the controller associated with the log entry.
    /// </summary>
    public string Controller { get; set; }
    /// <summary>
    /// Gets or sets the log level of the entry.
    /// </summary>
    public string Level { get; set; }
    /// <summary>
    /// Gets or sets the log message.
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// Gets or sets the URL associated with the log entry.
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// Gets or sets the action associated with the log entry.
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// Creates a new <see cref="LogEntry"/> instance by parsing the specified string.
    /// </summary>
    /// <param name="text">The string representation of the log entry.</param>
    /// <returns>A new <see cref="LogEntry"/> instance.</returns>
    internal static LogEntry FromString(string text) {
      string[] arr = text.Split('|');
      LogEntry entry = new LogEntry();
      entry.Time = DateTime.ParseExact(arr[0], "yyyy-MM-dd HH:mm:ss.ffff", System.Globalization.CultureInfo.InvariantCulture);
      entry.Id = string.IsNullOrWhiteSpace(arr[1]) ? 0 : int.Parse(arr[1]);
      entry.Level = arr[2];
      entry.Controller = arr[3];
      entry.Message = arr[4];
      entry.Url = arr[5].Substring(5); 
      entry.Action = arr[6].Substring(8);
      return entry;
    }
  }
}
