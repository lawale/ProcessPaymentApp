using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OlawaleFiledApp.Core.Domain
{
    public class PaymentState : BaseEntity
    {
        [Required]
        public PaymentResult PaymentResult { get; set; }
        
        public string? Reason { get; set; }

        [Required]
        public string PaymentGatewayType { get; set; } = null!;
        
        [Required]
        public Guid PaymentId { get; set; }
        
        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; } = null!;
    }
}