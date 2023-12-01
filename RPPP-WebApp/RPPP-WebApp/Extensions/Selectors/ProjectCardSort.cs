using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  public static class ProjectCardSort {

    public static IQueryable<ProjectCard> ApplySort(this IQueryable<ProjectCard> query, int sort, bool ascending) {
      Expression<Func<ProjectCard, object>> orderSelector = sort switch {
        1 => o => o.Iban,
        2 => o => o.Balance,
        3 => o => o.ActivationDate,
        4 => o => o.Oib,
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
