using System.Text;

namespace RPPP_WebApp.Extensions {
  /// <summary>
  /// Extension class for working with Exception messages.
  /// </summary>
  public static class ExceptionExtensions {
    /// <summary>
    /// Retrieves the complete exception message including inner exceptions.
    /// </summary>
    /// <param name="exc">The exception object.</param>
    /// <returns>The complete exception message.</returns>
    public static string CompleteExceptionMessage(this Exception exc) {
      StringBuilder sb = new StringBuilder();
      while (exc != null) {
        sb.AppendLine(exc.Message);
        exc = exc.InnerException;
      }
      return sb.ToString();
    }
  }
}
