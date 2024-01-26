using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{/// <summary>
 /// Extension class for sorting operations on IQueryable of ProjectPartner entities.
 /// </summary>
    public static class ProjectPartnerSort
    {/// <summary>
     /// Applies sorting to the IQueryable of ProjectPartner entities based on the specified sort order and direction.
     /// </summary>
     /// <param name="query">The IQueryable of ProjectPartner entities.</param>
     /// <param name="sort">The sort order.</param>
     /// <param name="ascending">True for ascending order, false for descending order.</param>
     /// <returns>The sorted IQueryable of ProjectPartner entities.</returns>
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
