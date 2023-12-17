using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class WorkerSort
    {
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
