using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class ProjectRoleSort
    {
        public static IQueryable<ProjectRole> ApplySort(this IQueryable<ProjectRole> query, int sort, bool ascending)
        {
            Expression<Func<ProjectRole, object>> orderSelector = sort switch
            {
                1 => o => o.Name,
                _ => null
            };

            if (orderSelector != null)
            {
                query = ascending ?
                  query.OrderBy(orderSelector) :
                  query.OrderByDescending(orderSelector);
            }

            return query;
        }
    }
}
