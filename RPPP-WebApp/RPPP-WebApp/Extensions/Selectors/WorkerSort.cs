using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{  /// <summary>
   /// Extension class for sorting operations on IQueryable of Worker entities.
   /// </summary>
    public static class WorkerSort

    {    /// <summary>
         /// Applies sorting to the IQueryable of Worker entities based on the specified sort order and direction.
         /// </summary>
         /// <param name="query">The IQueryable of Worker entities.</param>
         /// <param name="sort">The sort order.</param>
         /// <param name="ascending">True for ascending order, false for descending order.</param>
         /// <returns>The sorted IQueryable of Worker entities.</returns>
        public static IQueryable<Worker> ApplySort(this IQueryable<Worker> query, int sort, bool ascending)
        {
            Expression<Func<Worker, object>> orderSelector = sort switch
            {
                1 => o => o.Email,
                2 => o => o.FirstName,
                3 => o => o.LastName,
                4 => o => o.PhoneNumber,
                5 => o => o.Organization,
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
