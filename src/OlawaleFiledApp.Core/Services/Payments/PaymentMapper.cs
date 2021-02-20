using OlawaleFiledApp.Core.Domain;
using OlawaleFiledApp.Core.Models.Payloads;
using OlawaleFiledApp.Core.Models.Resources;

namespace OlawaleFiledApp.Core.Services.Payments
{
    public static class PaymentMapper
    {
        public static Payment ConvertFrom(PaymentPayload payload)
        {
            return new Payment
            {
                Amount = payload.Amount, CardHolder = payload.CardHolder,
                CreditCardNumber = payload.CreditCardNumber, ExpirationDate = payload.ExpirationDate,
                SecurityCode = payload.SecurityCode, State = PaymentState.Pending
            };
        }

        public static PaymentResource ConvertFrom(Payment payment)
        {
            return new PaymentResource
            {
                PaymentId = payment.Id,
                Amount = payment.Amount,
                CardHolder = payment.CardHolder,
                CreditCardNumber = payment.CreditCardNumber,
                ExpirationDate = payment.ExpirationDate,
                SecurityCode = payment.SecurityCode,
                PaymentState = payment.State.ToString()
            };
        }
    }
}