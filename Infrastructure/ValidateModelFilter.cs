using System;
using Games.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Games.Infrastructure {
    public class ValidateModelAttribute : Attribute, IActionFilter {
        public void OnActionExecuted(ActionExecutedContext context) { }

        public void OnActionExecuting(ActionExecutingContext context) {
            if (!context.ModelState.IsValid) {
                context.Result = new BadRequestObjectResult(new ApiError {
                    Message = "The model is invalid."
                });
            }
        }
    }
}