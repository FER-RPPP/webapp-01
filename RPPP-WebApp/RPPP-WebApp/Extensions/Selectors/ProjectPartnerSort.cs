using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class ProjectPartnerSort
    {
        public static IQueryable<ProjectPartner> ApplySort(this IQueryable<ProjectPartner> query, int sort, bool ascending)
        {
            Expression<Func<ProjectPartner, object>> orderSelector = sort switch
            {
                1 => o => o.Project,
                2 => o => o.Worker,
                3 => o => o.Role,
                4 => o => o.DateFrom,
                5 => o => o.DateTo,
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
