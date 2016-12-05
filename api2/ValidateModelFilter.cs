using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Games {
    public class ValidateModelAttribute : Attribute, IActionFilter {
        public void OnActionExecuted(ActionExecutedContext context) { }

        public void OnActionExecuting(ActionExecutingContext context) {
            if (!context.ModelState.IsValid) {
                context.Result = new BadRequestResult();
            }
        }
    }
}