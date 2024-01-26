using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
    /// <summary>
    /// Extension class for sorting operations on IQueryable of LaborType entities.
    /// </summary>
    public static class LaborTypeSort {
    /// <summary>
    /// Applies sorting to the IQueryable of LaborType entities based on the specified sort order and direction.
    /// </summary>
    /// <param name="query">The IQueryable of LaborType entities.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">True for ascending order, false for descending order.</param>
    /// <returns>The sorted IQueryable of LaborType entities.</returns>
    public static IQueryable<LaborType> ApplySort(this IQueryable<LaborType> query, int sort, bool ascending) {
      Expression<Func<LaborType, object>> orderSelector = sort switch {
        1 => o => o.Type,
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
