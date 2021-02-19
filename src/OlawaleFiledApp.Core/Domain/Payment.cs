using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OlawaleFiledApp.Core.Domain
{
    public class Payment : BaseEntity
    {
        [Required]
        public string CreditCardNumber { get; set; } = null!;
        
        [Required, StringLength(24)]
        public string CardHolder { get; set; } = null!;
        
        [Required, DataType(DataType.Date)]
        public DateTime ExpirationDate { get; set; }
        
        [StringLength(3)]
        public string? SecurityCode { get; set; }
        
        public PaymentState State { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
    }
}