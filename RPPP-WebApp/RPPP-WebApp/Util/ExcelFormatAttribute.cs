using System;

namespace RPPP_WebApp.Util
{
  /// <summary>
  /// Attribute used to specify the Excel format for a property.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class ExcelFormatAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the Excel format for the property.
    /// </summary>
    public string ExcelFormat { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelFormatAttribute"/> class with the specified format.
    /// </summary>
    /// <param name="format">The Excel format to be applied to the property.</param>

    public ExcelFormatAttribute(string format)
    {
      ExcelFormat = format;
    }
  }
}
