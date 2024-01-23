using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  /// <summary>
  /// Extension class for sorting operations on IQueryable of TransactionType entities.
  /// </summary>
  public static class TransactionTypeSort {
    /// <summary>
    /// Applies sorting to the IQueryable of TransactionType entities based on the specified sort order and direction.
    /// </summary>
    /// <param name="query">The IQueryable of TransactionType entities.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">True for ascending order, false for descending order.</param>
    /// <returns>The sorted IQueryable of TransactionType entities.</returns>
    public static IQueryable<TransactionType> ApplySort(this IQueryable<TransactionType> query, int sort, bool ascending) {
      Expression<Func<TransactionType, object>> orderSelector = sort switch {
        1 => o => o.TypeName,
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
