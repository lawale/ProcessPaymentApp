using System;
using System.ComponentModel.DataAnnotations;

namespace OlawaleFiledApp.Core.Models.Validations
{
    public class DateAttribute : ValidationAttribute
    {
        private readonly DateTime validDate;
        private readonly ValidationComparison comparison;

        public DateAttribute(ValidationComparison comparison)
        {
            this.comparison = comparison;
            validDate = DateTime.UtcNow;
        }

        public override bool IsValid(object? value)
        {
            if (value is DateTime date)
            {
                ErrorMessage ??= $"Invalid Value For {ErrorMessageResourceName ?? "Date Property"}";
                return comparison switch
                {
                    ValidationComparison.Greater => date.ToUniversalTime().Date > validDate.Date,
                    ValidationComparison.Less => date.ToUniversalTime().Date < validDate.Date,
                    _ => false
                };
            }
            
            ErrorMessage = $"{ErrorMessageResourceName ?? "Date Property" } is required";
            return false;

        }
    }

}