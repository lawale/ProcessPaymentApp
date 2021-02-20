namespace OlawaleFiledApp.Core.Models.Resources
{
    public class ObjectResource<TResponse> : StatusResource
    {
        public TResponse? Data { get; set; }
    }
}