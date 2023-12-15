using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  public static class LaborTypeSort {
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
