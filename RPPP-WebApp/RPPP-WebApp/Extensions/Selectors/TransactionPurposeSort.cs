using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  public static class TransactionPurposeSort {

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
