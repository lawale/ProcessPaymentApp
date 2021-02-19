using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OlawaleFiledApp.Api.Validation
{
    public class PayloadValidationActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if(!context.ModelState.IsValid)
            {
                context.Result = new ObjectResult(new ValidationResource(context.ModelState))
                {
                    StatusCode =  StatusCodes.Status400BadRequest
                };
            }
        }
    }
}