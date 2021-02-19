using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OlawaleFiledApp.Core.Services.Payments.Gateways.Implementations
{
    public class CheapPaymentGateway : BasePaymentGateway
    {
        protected override string GatewayEndpoint { get; }
        public override PaymentType Type { get; } = PaymentType.Cheap;
        
        public CheapPaymentGateway(ILogger<CheapPaymentGateway> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
            :base(logger, httpClientFactory)
        {
            GatewayEndpoint = configuration["GatewayUrls:Cheap"];
        }
    }
}