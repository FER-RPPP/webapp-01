using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
    /// <summary>
    /// Extension class for sorting operations on IQueryable of LaborDiary entities.
    /// </summary>
    public static class LaborDiarySort {
    /// <summary>
    /// Applies sorting to the IQueryable of LaborDiary entities based on the specified sort order and direction.
    /// </summary>
    /// <param name="query">The IQueryable of LaborDiary entities.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">True for ascending order, false for descending order.</param>
    /// <returns>The sorted IQueryable of LaborDiary entities.</returns>
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
