using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
    /// <summary>
    /// Extension class for sorting operations on IQueryable of Project entities.
    /// </summary>
    public static class ProjectMD4Sort {
    /// <summary>
    /// Applies sorting to the IQueryable of Project entities based on the specified sort order and direction.
    /// </summary>
    /// <param name="query">The IQueryable of Project entities.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">True for ascending order, false for descending order.</param>
    /// <returns>The sorted IQueryable of Project entities.</returns>
    public static IQueryable<Project> ApplySort(this IQueryable<Project> query, int sort, bool ascending) {
      Expression<Func<Project, object>> orderSelector = sort switch {
        1 => o => o.Name,
        2 => o => o.Type,
        3 => o => o.Owner.Name + o.Owner.Surname + o.Owner.Oib,
        4 => o => o.Client.FirstName + o.Client.LastName + o.Client.Oib,
        5 => o => o.CardId,
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
