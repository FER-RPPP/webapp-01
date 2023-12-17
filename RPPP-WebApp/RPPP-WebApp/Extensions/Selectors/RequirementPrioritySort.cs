using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class RequirementPrioritySort
    {
        public static IQueryable<RequirementPriority> ApplySort(this IQueryable<RequirementPriority> query, int sort, bool ascending)
        {
            Expression<Func<RequirementPriority, object>> orderSelector = sort switch
            {
                1 => o => o.Id,
                2 => o => o.Type,
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
