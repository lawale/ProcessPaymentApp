using System;

namespace OlawaleFiledApp.Core.Data.Exceptions
{
    public class EntityTransactionException : Exception
    {
        public EntityTransactionException(string message,string transactionInformation = "") : base(message)
        {
            TransactionInformation = transactionInformation;
        }

        public EntityTransactionException(string message,Exception internalException, string transactionInformation = "") : base(message,internalException)
        {
            TransactionInformation = transactionInformation;
        }

        public string TransactionInformation { get; }
    }
}