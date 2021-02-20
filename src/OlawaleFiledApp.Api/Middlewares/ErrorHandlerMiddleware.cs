using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OlawaleFiledApp.Core.Data.Repositories;
using OlawaleFiledApp.Core.Models.Resources;

namespace OlawaleFiledApp.Api.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ErrorHandlerMiddleware> logger;
        private readonly IUnitOfWork unitOfWork;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IUnitOfWork unitOfWork)
        {
            this.next = next;
            this.logger = logger;
            this.unitOfWork = unitOfWork;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                if (unitOfWork.TransactionExists())
                {
                    logger.LogInformation("Rolling Back Uncommitted unit of work");
                    await unitOfWork.RollbackAsync();
                }
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