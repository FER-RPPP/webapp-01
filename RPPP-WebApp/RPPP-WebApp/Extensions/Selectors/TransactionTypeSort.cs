using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  public static class TransactionTypeSort {
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
