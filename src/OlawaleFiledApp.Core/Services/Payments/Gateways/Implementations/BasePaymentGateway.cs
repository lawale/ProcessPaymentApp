using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OlawaleFiledApp.Core.Models.Payloads;

namespace OlawaleFiledApp.Core.Services.Payments.Gateways.Implementations
{
    public abstract class BasePaymentGateway : IPaymentGateway
    {
        private readonly ILogger<BasePaymentGateway> logger;
        private readonly IHttpClientFactory httpClientFactory;
        protected abstract string GatewayEndpoint { get; }

        public BasePaymentGateway(ILogger<BasePaymentGateway> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }
        
        public abstract PaymentType Type { get; }

        public virtual async Task<bool> ChargeCardAsync(PaymentPayload payload)
        {
            var client = httpClientFactory.CreateClient(Type.ToString());
            var body  = JsonSerializer.Serialize(payload);
            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            
            logger.LogInformation("Payload built for request: {0}", body);
            logger.LogInformation("Request Url: {0}", GatewayEndpoint);
            logger.LogInformation("Request Headers: {0}", client.DefaultRequestHeaders);

            try
            {
                var response = await client.PostAsync(GatewayEndpoint, content);

                var result = await response.Content.ReadAsStringAsync();
                
                logger.LogInformation("Response From {0} Gateway: {1}", Type, result);

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Call to {0} Gateway failed", Type);
                return false;
            }
        }
    }
}