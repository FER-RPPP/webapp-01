using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  /// <summary>
  /// Extension class for sorting operations on IQueryable of Transaction entities.
  /// </summary>
  public static class TransactionSort {
    /// <summary>
    /// Applies sorting to the IQueryable of Transaction entities based on the specified sort order and direction.
    /// </summary>
    /// <param name="query">The IQueryable of Transaction entities.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">True for ascending order, false for descending order.</param>
    /// <returns>The sorted IQueryable of Transaction entities.</returns>
    public static IQueryable<Transaction> ApplySort(this IQueryable<Transaction> query, int sort, bool ascending) {
      Expression<Func<Transaction, object>> orderSelector = sort switch {
        1 => o => o.Iban,
        2 => o => o.Recipient,
        3 => o => o.Amount,
        4 => o => o.Date,
        5 => o => o.Type.TypeName,
        6 => o => o.Purpose.PurposeName,
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
