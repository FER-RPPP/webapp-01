using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors {
  public static class ProjectMD4Sort {
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
