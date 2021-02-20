using System;
using System.ComponentModel.DataAnnotations;

namespace OlawaleFiledApp.Core.Domain
{
    public class Payment : BaseEntity
    {
        [Required, DataType(DataType.CreditCard)]
        public string CreditCardNumber { get; set; } = null!;
        
        [Required, StringLength(24)]
        public string CardHolder { get; set; } = null!;
        
        [Required, DataType(DataType.Date)]
        public DateTime ExpirationDate { get; set; }
        
        [StringLength(3, MinimumLength = 3)]
        public string? SecurityCode { get; set; }
        
        [EnumDataType(typeof(PaymentState))]
        public PaymentResult State { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
    }
}