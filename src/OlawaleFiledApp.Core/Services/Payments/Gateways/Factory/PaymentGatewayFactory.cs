using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OlawaleFiledApp.Core.Services.Payments.Gateways.Exceptions;

namespace OlawaleFiledApp.Core.Services.Payments.Gateways.Factory
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly ILogger<PaymentGatewayFactory> logger;
        private readonly IEnumerable<IPaymentGateway> paymentGateways;

        public PaymentGatewayFactory(IEnumerable<IPaymentGateway> paymentGateways, ILogger<PaymentGatewayFactory> logger)
        {
            this.paymentGateways = paymentGateways;
            this.logger = logger;
        }
        
        public IPaymentGateway? ResolveGateway(PaymentType paymentType)
        {
            logger.LogInformation("Resolving Gateway for Type {0}", paymentType);

            if (!paymentGateways.Any())
            {
                logger.LogCritical("No Payment Gateway was found");
                throw new PaymentGatewayException("No Payment Gateway Found");
            }

            var paymentGateway = paymentGateways.FirstOrDefault(x => x.Type == paymentType);

            if (paymentGateway is null)
            {
                logger.LogWarning("No Gateway resolved for type {0}", paymentType);
                return null;
            }
            
            logger.LogInformation("Successfully resolved gateway for {0}", paymentType);
            return paymentGateway;
        }
    }
}