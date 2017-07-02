using System.Linq;
using Games.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Games.Infrastructure
{
    public class ValidateModelFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context) { }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
                return;

            var error = new ApiValidationError
            {
                Message = "The input was invalid.",
                Errors = context.ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
            context.Result = new BadRequestObjectResult(error);
        }
    }

    public class HandleExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var e = context.Exception;
            var error = new ApiError { Message = e.Message };

            if (e is UnauthorizedException)
                context.Result = new UnauthorizedObjectResult(error);
            else if (e is NotFoundException)
                context.Result = new NotFoundObjectResult(error);
            else if (e is BadRequestException)
                context.Result = new BadRequestObjectResult(error);
            else
                context.Result = new InternalServerErrorObjectResult(error);
        }

        private class InternalServerErrorObjectResult : ObjectResult
        {
            public InternalServerErrorObjectResult(object error) : base(error)
            {
                StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        private class NotFoundObjectResult : ObjectResult
        {
            public NotFoundObjectResult(object error) : base(error)
            {
                StatusCode = StatusCodes.Status404NotFound;
            }
        }

        private class UnauthorizedObjectResult : ObjectResult
        {
            public UnauthorizedObjectResult(object error) : base(error)
            {
                StatusCode = StatusCodes.Status401Unauthorized;
            }
        }
    }
}
