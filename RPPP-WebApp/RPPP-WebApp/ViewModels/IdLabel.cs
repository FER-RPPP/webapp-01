using System.Text.Json.Serialization;

namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a class that combines an identifier and a label.
  /// </summary>
  public class IdLabel {
    /// <summary>
    /// Gets or sets the label associated with the identifier.
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; set; }
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }
    /// <summary>
    /// Initializes a new instance of the <see cref="IdLabel"/> class.
    /// </summary>
    public IdLabel() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="IdLabel"/> class with the specified identifier and label.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="label">The label.</param>
    public IdLabel(string id, string label) {
      Id = id;
      Label = label;
    }
  }
}
