using System;

namespace OlawaleFiledApp.Core.Models.Resources
{
    public class PaymentResource
    {
        public string PaymentState { get; set; } = null!;
        public DateTime ExpirationDate { get; set; }
        public string CreditCardNumber { get; set; } = null!;
        public string CardHolder { get; set; } = null!;
        public decimal Amount { get; set; }
        public Guid PaymentId { get; set; }
        public string? SecurityCode { get; set; }
    }
}