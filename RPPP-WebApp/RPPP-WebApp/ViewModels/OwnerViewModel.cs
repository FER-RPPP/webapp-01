using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels {
  /// <summary>
  /// Represents a view model for owners, containing a collection of owners and paging information.
  /// </summary>
  public class OwnerViewModel {
    /// <summary>
    /// Gets or sets the collection of owners.
    /// </summary>
    public IEnumerable<Owner> Owner {  get; set; }
    /// <summary>
    /// Gets or sets the paging information for the owner view model.
    /// </summary>
    public PagingInfo PagingInfo { get; set; }
  }
}
