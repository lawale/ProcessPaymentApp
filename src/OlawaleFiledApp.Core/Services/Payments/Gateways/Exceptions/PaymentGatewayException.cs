using System;

namespace OlawaleFiledApp.Core.Services.Payments.Gateways.Exceptions
{
    public class PaymentGatewayException : Exception
    {
        public PaymentGatewayException(string message) : base(message)
        {
        }
    }
}