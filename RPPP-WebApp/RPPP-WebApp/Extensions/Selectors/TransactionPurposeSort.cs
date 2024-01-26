using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  /// <summary>
  /// Extension class for sorting operations on IQueryable of TransactionPurpose entities.
  /// </summary>
  public static class TransactionPurposeSort {

    /// <summary>
    /// Applies sorting to the IQueryable of TransactionPurpose entities based on the specified sort order and direction.
    /// </summary>
    /// <param name="query">The IQueryable of TransactionPurpose entities.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">True for ascending order, false for descending order.</param>
    /// <returns>The sorted IQueryable of TransactionPurpose entities.</returns>
    public static IQueryable<TransactionPurpose> ApplySort(this IQueryable<TransactionPurpose> query, int sort, bool ascending) {
      Expression<Func<TransactionPurpose, object>> orderSelector = sort switch {
        1 => o => o.PurposeName,
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
