using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
    /// <summary>
    /// Extension class for sorting operations on IQueryable of ProjectWork entities.
    /// </summary>
    public static class ProjectWorkSort {
    /// <summary>
    /// Applies sorting to the IQueryable of ProjectWork entities based on the specified sort order and direction.
    /// </summary>
    /// <param name="query">The IQueryable of ProjectWork entities.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">True for ascending order, false for descending order.</param>
    /// <returns>The sorted IQueryable of ProjectWork entities.</returns>
    public static IQueryable<ProjectWork> ApplySort(this IQueryable<ProjectWork> query, int sort, bool ascending) {
      Expression<Func<ProjectWork, object>> orderSelector = sort switch {
        1 => o => o.Title,
        2 => o => o.Project.Name,
        3 => o => o.Assignee.FirstName + o.Assignee.LastName,
        4 => o => o.Description,
        5 => o => o.LaborDiary.OrderBy(l => l.Date).Select(l => l.Date).FirstOrDefault(),
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
