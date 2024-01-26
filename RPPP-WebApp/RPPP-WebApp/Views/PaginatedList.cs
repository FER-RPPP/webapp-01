using Microsoft.EntityFrameworkCore;

namespace RPPP_WebApp.Views
{
  /// <summary>
  /// Represents a paginated list of items.
  /// </summary>
  /// <typeparam name="T">The type of items in the paginated list.</typeparam>
  public class PaginatedList<T> : List<T>
    {
    /// <summary>
    /// Gets the index of the current page.
    /// </summary>
    public int PageIndex { get; private set; }
    /// <summary>
    /// Gets the total number of pages in the paginated list.
    /// </summary>
    public int TotalPages { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class.
    /// </summary>
    /// <param name="items">The list of items in the current page.</param>
    /// <param name="count">The total count of items in the source.</param>
    /// <param name="pageIndex">The index of the current page.</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            AddRange(items);
        }

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage
        {
            get
            {
                return PageIndex > 1;
            }
        }

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage
        {
            get
            {
                return PageIndex < TotalPages;
            }
        }

    /// <summary>
    /// Creates a new instance of the <see cref="PaginatedList{T}"/> asynchronously.
    /// </summary>
    /// <param name="source">The source queryable to paginate.</param>
    /// <param name="pageIndex">The index of the current page.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>An instance of <see cref="PaginatedList{T}"/>.</returns>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }

}
