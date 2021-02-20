using System;
using System.ComponentModel.DataAnnotations;
using OlawaleFiledApp.Core.Models.Validations;

namespace OlawaleFiledApp.Core.Models.Payloads
{
    public class PaymentPayload
    {
        [Required(ErrorMessage = "Credit Card Number Is Required")]
        [CreditCardNumber(ErrorMessage = "Invalid Credit Card Number")]
        public string CreditCardNumber { get; set; } = null!;
        
        [Required(ErrorMessage = "Card Holder Name Is Required")]
        [StringLength(24, ErrorMessage = "Card Holder Name Should Not Be More Than 24 Characters Long")]
        public string CardHolder { get; set; } = null!;
        
        [Date(ValidationComparison.Greater)]
        public DateTime ExpirationDate { get; set; }
        
        [StringLength(3, MinimumLength = 3)]
        public string? SecurityCode { get; set; }
        
        [Decimal(0,ValidationComparison.Greater)]
        public decimal Amount { get; set; }
    }
}