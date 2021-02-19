using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OlawaleFiledApp.Core.Services.Payments.Gateways.Implementations
{
    public class ExpensivePaymentGateway : BasePaymentGateway
    {
        protected override string GatewayEndpoint { get; }
        
        public override PaymentType Type { get; } = PaymentType.Expensive;

        public ExpensivePaymentGateway(ILogger<ExpensivePaymentGateway> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        :base(logger, httpClientFactory)
        {
            GatewayEndpoint = configuration["GatewayUrls:Expensive"];
        }
    }
}