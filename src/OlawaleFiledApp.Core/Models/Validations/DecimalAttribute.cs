using System.ComponentModel.DataAnnotations;
using static System.Decimal;

namespace OlawaleFiledApp.Core.Models.Validations
{
    public class DecimalAttribute : ValidationAttribute
    {
        private readonly decimal validAmount;
        private readonly ValidationComparison comparison;

        public DecimalAttribute(int validAmount, ValidationComparison comparison)
        {
            if (!TryParse(validAmount.ToString(), out this.validAmount))
                this.validAmount = Zero;
            this.comparison = comparison;
        }

        public override bool IsValid(object? value)
        {
            if (value is decimal amount)
            {
                ErrorMessage ??= $"Invalid Value For {ErrorMessageResourceName ?? "Decimal Property"}";
                return comparison switch
                {
                    ValidationComparison.Greater => amount > validAmount,
                    ValidationComparison.Less => amount < validAmount,
                    _ => false
                };
            }
            ErrorMessage = $"{ErrorMessageResourceName ?? "Decimal Property" } is required";
            return false;
        }
    }
}