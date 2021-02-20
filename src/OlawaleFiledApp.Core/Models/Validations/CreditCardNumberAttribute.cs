using System;
using System.ComponentModel.DataAnnotations;
using OlawaleFiledApp.Core.Models.Constants;

namespace OlawaleFiledApp.Core.Models.Validations
{
    public class CreditCardNumberAttribute : RegularExpressionAttribute
    {
        public CreditCardNumberAttribute() : base(StringConstants.CreditCardRegex)
        {
        }

        public override bool IsValid(object? value)
        {
            if (value is not string ccn || string.IsNullOrEmpty(ccn)) return false;
            ccn = ccn.Replace(" ", String.Empty).Replace("-", string.Empty);
            return base.IsValid(ccn);
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return base.IsValid(value, validationContext);
        }
    }
}