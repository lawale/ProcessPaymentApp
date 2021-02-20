using System;

namespace OlawaleFiledApp.Core.Models.Constants
{
    /// <summary>
    /// Contains general category of constant values. Not limited to strings
    /// </summary>
    public static class StringConstants
    {
        public static readonly string ConnectionString = Guid.NewGuid().ToString();

        public const string CreditCardRegex = @"^(?:(4[0-9]{12}(?:[0-9]{3})?)|(5[1-5][0-9]{14})|(6(?:011|5[0-9]{2})[0-9]{12})|(3[47][0-9]{13})|(3(?:0[0-5]|[68][0-9])[0-9]{11})|((?:2131|1800|35[0-9]{3})[0-9]{11})|(((506(0|1))|(507(8|9))|(6500))[0-9]{12,15}))$";
    }
}