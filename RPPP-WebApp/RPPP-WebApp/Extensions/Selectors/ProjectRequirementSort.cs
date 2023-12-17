using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class ProjectRequirementSort
    {
        public static IQueryable<ProjectRequirement> ApplySort(this IQueryable<ProjectRequirement> query, int sort, bool ascending)
        {
            Expression<Func<ProjectRequirement, object>> orderSelector = sort switch
            {
                1 => o => o.Id,
                2 => o => o.Type,
                3 => o => o.Description,
                4 => o => o.Project.Name,
                5 => o => o.RequirementPriority.Type,
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
