using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{/// <summary>
 /// Extension class for sorting operations on IQueryable of Organization entities.
 /// </summary>
    public static class OrganizationSort
    {/// <summary>
     /// Applies sorting to the IQueryable of Organization entities based on the specified sort order and direction.
     /// </summary>
     /// <param name="query">The IQueryable of Organization entities.</param>
     /// <param name="sort">The sort order.</param>
     /// <param name="ascending">True for ascending order, false for descending order.</param>
     /// <returns>The sorted IQueryable of Organization entities.</returns>
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
