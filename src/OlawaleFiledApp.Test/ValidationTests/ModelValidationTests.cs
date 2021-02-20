using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;
using OlawaleFiledApp.Core.Models.Payloads;
using OlawaleFiledApp.Core.Models.Validations;

namespace OlawaleFiledApp.Test.ValidationTests
{
    public class ModelValidationTests
    {
        public static IEnumerable<string> CCNs
        {
            get
            {
                yield return "4111 1111 1111 1111"; //visa
                yield return "5500 0000 0000 0004"; //Mastercard
                yield return "3400-0000-0000-009"; //American Express
                yield return "3000 0000 0000 04"; //Diner's Club
                yield return "6011-0000-0000-0004"; //Discover
                yield return "3530111333300000"; //JCB
                yield return "5061-0001-1010-1002-883"; //Verve
            }
        }

        public static IEnumerable<ValidationComparison> Comparisons
        {
            get
            {
                yield return ValidationComparison.Greater;
                yield return ValidationComparison.Less;
            }
        }
        
        public static IEnumerable<int> ComparisonAmounts
        {
            get
            {
                yield return 23;
                yield return 2300;
            }
        }

        public static IEnumerable<decimal> Amounts
        {
            get
            {
                yield return 400M;
                yield return 800M;
            }
        }
        
        public static IEnumerable<DateTime> Dates
        {
            get
            {
                yield return DateTime.Now.AddYears(3);
                yield return DateTime.Now.AddYears(-5);
            }
        }

        public static IEnumerable<PaymentPayload> InvalidPayloads
        {
            get
            {
                //Invalid Amount
                yield return new PaymentPayload
                {
                    Amount = -100, CreditCardNumber = CCNs.First(), CardHolder = "Olawale Lawal",
                    ExpirationDate = DateTime.Now.AddYears(2)
                };
                
                //Invalid CCN
                yield return new PaymentPayload
                {
                    Amount = 500M, CreditCardNumber = "00000000000", CardHolder = "Olawale Lawal",
                    ExpirationDate = DateTime.Now.AddYears(2)
                };
                
                //Invalid CardHolderName
                yield return new PaymentPayload
                {
                    Amount = 500M, CreditCardNumber = CCNs.First(), CardHolder = "Olawale Lawal".PadRight(26,'L'),
                    ExpirationDate = DateTime.Now.AddYears(2)
                };
                
                //Invalid Exp Date
                yield return new PaymentPayload
                {
                    Amount = 500M, CreditCardNumber = CCNs.First(), CardHolder = "Olawale Lawal",
                    ExpirationDate = DateTime.Now.AddYears(-2)
                };
            }
        }

        [Test]
        [TestCaseSource(nameof(CCNs))]
        public void Test_CreditCardNumber(string ccn)
        {
            var attribute = new CreditCardNumberAttribute();

            var result = attribute.IsValid(ccn);
            
            Assert.IsTrue(result);
        }

        [Test, Sequential]
        public void Test_DateAttribute([ValueSource(nameof(Comparisons))] ValidationComparison comparison, [ValueSource(nameof(Dates))] DateTime date)
        {
            var attribute = new DateAttribute(comparison);

            var result = attribute.IsValid(date);
            
            Assert.IsTrue(result);
        }

        [Test, Sequential]
        public void Test_DecimalAttribute([ValueSource(nameof(Comparisons))] ValidationComparison comparison, [ValueSource(nameof(Amounts))] decimal amount, [ValueSource(nameof(ComparisonAmounts))] int comparisonAmount)
        {
            var attribute = new DecimalAttribute(comparisonAmount, comparison);
            
            var result = attribute.IsValid(amount);
            
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_PaymentPayload()
        {
            var payload = new PaymentPayload
            {
                Amount = 500M, CreditCardNumber = CCNs.First(), CardHolder = "Olawale Lawal",
                ExpirationDate = DateTime.Now.AddYears(2)
            };
            
            var result = Validator.TryValidateObject(payload, new ValidationContext(payload, null, null), null, true);
            Assert.True(result);
        }
        
        [Test]
        [TestCaseSource(nameof(InvalidPayloads))]
        public void Test_PaymentInvalidPayload(PaymentPayload payload)
        {
            
            var result = Validator.TryValidateObject(payload, new ValidationContext(payload, null, null), null, true);
            Assert.IsFalse(result);
        }
    }
}