using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  public static class OwnerSort {
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
