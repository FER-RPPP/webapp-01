using RPPP_WebApp.Model;
using System.Linq.Expressions;
using TaskStatus = RPPP_WebApp.Model.TaskStatus;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class TaskStatusSort
    {
        public static IQueryable<TaskStatus> ApplySort(this IQueryable<TaskStatus> query, int sort, bool ascending)
        {
            Expression<Func<TaskStatus, object>> orderSelector = sort switch
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
