using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class OrganizationSort
    {
        public static IQueryable<Organization> ApplySort(this IQueryable<Organization> query, int sort, bool ascending)
        {
            Expression<Func<Organization, object>> orderSelector = sort switch
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
