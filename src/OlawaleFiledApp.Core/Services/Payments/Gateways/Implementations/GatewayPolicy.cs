using System;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace OlawaleFiledApp.Core.Services.Payments.Gateways.Implementations
{
    public static class GatewayPolicy
    {
        public static IAsyncPolicy<HttpResponseMessage> PremiumRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}