using System.Text.Json.Serialization;

namespace RPPP_WebApp.ViewModels
{
  /// <summary>
  /// Represents different types of messages used in the application.
  /// </summary>
  public class MessageType
  {
    /// <summary>
    /// Represents a success message type.
    /// </summary>
    public const string Success = "success";
    /// <summary>
    /// Represents an info message type.
    /// </summary>
    public const string Info = "success";
    /// <summary>
    /// Represents a warning message type.
    /// </summary>
    public const string Warning = "warning";
    /// <summary>
    /// Represents an error message type.
    /// </summary>
    public const string Error = "error";
  }
  /// <summary>
  /// Represents a response message with an associated message type.
  /// </summary>
  public record ActionResponseMessage([property:JsonPropertyName("messageType")] string MessageType, [property: JsonPropertyName("message")]  string Message);
}
