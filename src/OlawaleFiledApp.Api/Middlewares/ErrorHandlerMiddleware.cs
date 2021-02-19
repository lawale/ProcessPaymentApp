using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OlawaleFiledApp.Core.Models.Resources;

namespace OlawaleFiledApp.Api.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ErrorHandlerMiddleware> logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                response.StatusCode = e is NotImplementedException ? StatusCodes.Status501NotImplemented : StatusCodes.Status500InternalServerError;
            
                logger.LogError(e, "Global Exception Caught");
                if(e.InnerException is not null)
                    logger.LogError(e, "Inner Exception of Global Exception Caught");

                var result = new StatusResource {Message = "Service Error", Status = false};

                await response.WriteAsJsonAsync(result);
            }
        }
    }
}