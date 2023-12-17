using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class RequirementTaskSort
    {
        public static IQueryable<RequirementTask> ApplySort(this IQueryable<RequirementTask> query, int sort, bool ascending)
        {
            Expression<Func<RequirementTask, object>> orderSelector = sort switch
            {
                1 => o => o.Id,
                2 => o => o.PlannedStartDate,
                3 => o => o.PlannedEndDate,
                4 => o => o.ActualStartDate,
                5 => o => o.ActualEndDate,
                6 => o => o.ProjectWork.Title,
                7 => o => o.ProjectRequirement.Description,
                8 => o => o.TaskStatus.Type,
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
