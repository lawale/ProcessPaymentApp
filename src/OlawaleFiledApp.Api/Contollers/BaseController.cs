using Microsoft.AspNetCore.Mvc;
using OlawaleFiledApp.Core.Models.Resources;

namespace OlawaleFiledApp.Api.Contollers
{
    public abstract class BaseController : Controller
    {
        protected ActionResult<TRes> HandleResponse<TRes>(TRes result) where TRes:StatusResource
        {
            return result.ResponseType switch
            {
                ResponseType.Success => Ok(result),
                ResponseType.Unauthorized => Unauthorized(result),
                ResponseType.NoData => NotFound(result),
                _ => BadRequest(result)
            };
        }
        
        protected ActionResult<TRes> HandleResponse<TRes>(string actionName, TRes result) where TRes:StatusResource
        {
            return result.ResponseType switch
            {
                ResponseType.Created => CreatedAtAction(actionName, result),
                _ => HandleResponse(result)
            };
        }
        
    }
}