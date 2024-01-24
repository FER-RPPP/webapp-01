﻿using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{/// <summary>
 /// Extension class for sorting operations on IQueryable of ProjectRole entities.
 /// </summary>
    public static class ProjectRoleSort
    { /// <summary>
      /// Applies sorting to the IQueryable of ProjectRole entities based on the specified sort order and direction.
      /// </summary>
      /// <param name="query">The IQueryable of ProjectRole entities.</param>
      /// <param name="sort">The sort order.</param>
      /// <param name="ascending">True for ascending order, false for descending order.</param>
      /// <returns>The sorted IQueryable of ProjectRole entities.</returns>
        public static IQueryable<ProjectRole> ApplySort(this IQueryable<ProjectRole> query, int sort, bool ascending)
        {
            Expression<Func<ProjectRole, object>> orderSelector = sort switch
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
