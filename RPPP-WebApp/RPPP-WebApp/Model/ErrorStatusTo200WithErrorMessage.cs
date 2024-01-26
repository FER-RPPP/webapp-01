using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.Extensions;

namespace RPPP_WebApp.Model
{
  /// <summary>
  /// Represents an exception filter attribute that converts error status to 200 with error message.
  /// </summary>
  public class ErrorStatusTo200WithErrorMessage : ExceptionFilterAttribute
    {
        private readonly ILogger<ErrorStatusTo200WithErrorMessage> logger;


    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorStatusTo200WithErrorMessage"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ErrorStatusTo200WithErrorMessage(ILogger<ErrorStatusTo200WithErrorMessage> logger)
        {
            this.logger = logger;
        }

    /// <summary>
    /// Called when an exception occurs.
    /// </summary>
    /// <param name="context">The exception context.</param>
    public override void OnException(ExceptionContext context)
        {
            string exceptionMessage = context.Exception.CompleteExceptionMessage();
            context.ExceptionHandled = true;
            JTableAjaxResult result = JTableAjaxResult.Error(exceptionMessage);
            context.Result = new OkObjectResult(result);
        }
    }
}
