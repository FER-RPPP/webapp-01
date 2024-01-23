using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Filters;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.TagHelpers {
  /// <summary>
  /// TagHelper for rendering pagination controls.
  /// </summary>
  [HtmlTargetElement("pager", Attributes = "page-info, page-action, page-title")]
  public class PagerTagHelper : TagHelper {

    private readonly IUrlHelperFactory urlHelperFactory;
    private readonly AppSettings appData;
    /// <summary>
    /// Initializes a new instance of the <see cref="PagerTagHelper"/> class.
    /// </summary>
    /// <param name="helperFactory">The <see cref="IUrlHelperFactory"/> used for generating URLs.</param>
    /// <param name="options">The <see cref="IOptionsSnapshot{TOptions}"/> containing the application settings.</param>
    public PagerTagHelper(IUrlHelperFactory helperFactory, IOptionsSnapshot<AppSettings> options) {
      urlHelperFactory = helperFactory;
      appData = options.Value;
    }

    /// <summary>
    /// Gets or sets the ViewContext for rendering the tag.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="PagingInfo"/> object containing paging information.
    /// </summary>
    public PagingInfo PageInfo { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IPageFilter"/> used for filtering pages.
    /// </summary>
    public IPageFilter PageFilter { get; set; }

    /// <summary>
    /// Gets or sets the page action to be used in URL generation.
    /// </summary>
    public string PageAction { get; set; }

    /// <summary>
    /// Gets or sets the title of the page.
    /// </summary>
    public string PageTitle { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output) {
      output.TagName = "nav";
      int offset = appData.PageOffset;
      TagBuilder paginationList = new TagBuilder("ul");
      paginationList.AddCssClass("pagination");

      if (PageInfo.CurrentPage - offset > 1)
      {
        var tag = BuildListItemForPage(1, "1..");
        paginationList.InnerHtml.AppendHtml(tag);
      }

      for (int i = Math.Max(1, PageInfo.CurrentPage - offset);
               i <= Math.Min(PageInfo.TotalPages, PageInfo.CurrentPage + offset);
               i++) {
        var tag = i == PageInfo.CurrentPage ? BuildListItemForCurrentPage(i) : BuildListItemForPage(i);
        paginationList.InnerHtml.AppendHtml(tag);
      }

      if (PageInfo.CurrentPage + offset < PageInfo.TotalPages)
      {
        var tag = BuildListItemForPage(PageInfo.TotalPages, ".. " + PageInfo.TotalPages);
        paginationList.InnerHtml.AppendHtml(tag);
      }

      output.Content.AppendHtml(paginationList);
    }

    private TagBuilder BuildListItemForPage(int i) {
      return BuildListItemForPage(i, i.ToString());
    }

    private TagBuilder BuildListItemForPage(int i, string text) {
      IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);

      TagBuilder a = new TagBuilder("a");
      a.InnerHtml.Append(text);
      a.Attributes["href"] = urlHelper.Action(PageAction, new {
        page = i,
        sort = PageInfo.Sort,
        ascending = PageInfo.Ascending,
        filter = PageFilter?.ToString()
      });
      a.AddCssClass("page-link");

      TagBuilder li = new TagBuilder("li");
      li.AddCssClass("page-item");
      li.InnerHtml.AppendHtml(a);
      return li;
    }
    private TagBuilder BuildListItemForCurrentPage(int page) {
      IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
      TagBuilder input = new TagBuilder("input");
      input.Attributes["type"] = "text";
      input.Attributes["value"] = page.ToString();
      input.Attributes["data-current"] = page.ToString();
      input.Attributes["data-min"] = "1";
      input.Attributes["data-max"] = PageInfo.TotalPages.ToString();
      input.Attributes["data-url"] = urlHelper.Action(PageAction, new {
        page = -1,
        sort = PageInfo.Sort,
        ascending = PageInfo.Ascending,
        filter = PageFilter?.ToString()
      });
      input.AddCssClass("page-link");
      input.AddCssClass("pagebox");

      if (!string.IsNullOrWhiteSpace(PageTitle)) {
        input.Attributes["title"] = PageTitle;
      }

      TagBuilder li = new TagBuilder("li");
      li.AddCssClass("page-item active");
      li.InnerHtml.AppendHtml(input);

      return li;
    }

  }
}
