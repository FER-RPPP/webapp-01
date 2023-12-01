using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  public static class TransactionSort {
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
