using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers {
  /// <summary>
  /// Controller for viewing log entries.
  /// </summary>
  public class ViewLogGOController : Controller {
    /// <summary>
    /// Displays the default view for log entries.
    /// </summary>
    /// <returns>The default view for log entries.</returns>
    public IActionResult Index() {
      return View();
    }

    /// <summary>
    /// Retrieves and displays log entries for a specified date.
    /// </summary>
    /// <param name="dan">The date for which to retrieve log entries.</param>
    /// <returns>The view displaying log entries for the specified date.</returns>
    public async Task<IActionResult> Show(DateTime dan) {
      ViewBag.Dan = dan;
      List<LogEntry> list = new List<LogEntry>();
      string format = dan.ToString("yyyy-MM-dd");
      string filename = Path.Combine(AppContext.BaseDirectory, $"logs/nlog-own-{format}.log");
      if (System.IO.File.Exists(filename)) {
        String previousEntry = string.Empty;
        using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
          using (StreamReader reader = new StreamReader(fileStream)) {
            string line;
            while ((line = await reader.ReadLineAsync()) != null) {
              if (line.StartsWith(format)) {
                if (previousEntry != string.Empty) {
                  LogEntry logEntry = LogEntry.FromString(previousEntry);
                  list.Add(logEntry);
                }
                previousEntry = line;
              }
              else {
                previousEntry += line;
              }
            }
          }
        }

        if (previousEntry != string.Empty) {
          LogEntry logEntry = LogEntry.FromString(previousEntry);
          list.Add(logEntry);
        }
      }
      list.Sort((a, b) => -a.Time.CompareTo(b.Time));
      return View(list);
    }
  }
}
