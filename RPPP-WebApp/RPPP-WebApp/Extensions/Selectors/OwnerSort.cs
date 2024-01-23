using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  /// <summary>
  /// Extension class for sorting operations on IQueryable of Owner entities.
  /// </summary>
  public static class OwnerSort {
    

    /// <summary>
    /// Applies sorting to the IQueryable of Owner entities based on the specified sort order and direction.
    /// </summary>
    /// <param name="query">The IQueryable of Owner entities.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">True for ascending order, false for descending order.</param>
    /// <returns>The sorted IQueryable of Owner entities.</returns>
    public static IQueryable<Owner> ApplySort(this IQueryable<Owner> query, int sort, bool ascending) {
      Expression<Func<Owner, object>> orderSelector = sort switch {
        1 => o => o.Oib,
        2 => o => o.Name,
        3 => o => o.Surname,
        _ => null
      };

      if (orderSelector != null) {
        query = ascending ?
               query.OrderBy(orderSelector) :
               query.OrderByDescending(orderSelector);
      }

      return query;
    }
  }
}
