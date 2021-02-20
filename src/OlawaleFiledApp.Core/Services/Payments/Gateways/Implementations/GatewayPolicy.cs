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
                .OrResult(msg =>
                {
                    Console.WriteLine("Call To Premium Gateway Returned Error Of {0}", msg.ReasonPhrase);
                    return !msg.IsSuccessStatusCode;
                })
                .WaitAndRetryAsync(3, retryAttempt =>
                {
                    Console.WriteLine("Attempt Number {0} at retrying premium", retryAttempt);
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                });
        }
    }
}