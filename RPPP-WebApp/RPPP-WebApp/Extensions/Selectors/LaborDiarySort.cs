using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  public static class LaborDiarySort {
    public static IQueryable<LaborDiary> ApplySort(this IQueryable<LaborDiary> query, int sort, bool ascending) {
      Expression<Func<LaborDiary, object>> orderSelector = sort switch {
        1 => o => o.Date,
        2 => o => o.Work.Title,
        3 => o => o.Worker.FirstName + o.Worker.LastName,
        4 => o => o.HoursSpent,
        5 => o => o.LaborType.Type,
        6 => o => o.LaborDescription,
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
