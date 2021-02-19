using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OlawaleFiledApp.Core.Models.Resources;

namespace OlawaleFiledApp.Api.Validation
{
    public class ValidationResource : StatusResource
    {
        public Dictionary<string,string> Errors { get; set; }
        
        public ValidationResource(ModelStateDictionary modelState)
        {
            var keyValuePairs = modelState.Keys
                .SelectMany(key =>
                    modelState[key].Errors.Select(x => new KeyValuePair<string, string>(key, x.ErrorMessage)));
            Errors = new Dictionary<string, string>(keyValuePairs);
            Message = "Validation Error has occurred on payload";
            Status = false;
            ResponseType = ResponseType.Failed;
        }
    }
}