using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  /// <summary>
  /// Extension class for sorting operations on IQueryable of ProjectCard entities.
  /// </summary>
  public static class ProjectCardSort {

    /// <summary>
    /// Applies sorting to the IQueryable of ProjectCard entities based on the specified sort order and direction.
    /// </summary>
    /// <param name="query">The IQueryable of ProjectCard entities.</param>
    /// <param name="sort">The sort order.</param>
    /// <param name="ascending">True for ascending order, false for descending order.</param>
    /// <returns>The sorted IQueryable of ProjectCard entities.</returns>
    public static IQueryable<ProjectCard> ApplySort(this IQueryable<ProjectCard> query, int sort, bool ascending) {
      Expression<Func<ProjectCard, object>> orderSelector = sort switch {
        1 => o => o.Iban,
        2 => o => o.Balance,
        3 => o => o.ActivationDate,
        4 => o => o.OibNavigation.Name,
        5 => o => o.Transaction.OrderBy(t => t.Recipient).Select(t => t.Recipient).FirstOrDefault(),
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
