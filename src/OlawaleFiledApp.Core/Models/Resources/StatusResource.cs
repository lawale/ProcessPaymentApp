using System.Text.Json.Serialization;

namespace OlawaleFiledApp.Core.Models.Resources
{
    public class StatusResource
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public ResponseType ResponseType { get; set; }
        
        public bool Status { get; set; }
        
        public string Message { get; set; } = null!;
    }
}