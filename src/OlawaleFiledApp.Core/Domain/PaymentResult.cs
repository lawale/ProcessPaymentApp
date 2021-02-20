using System;

namespace OlawaleFiledApp.Core.Domain
{
    [Flags]
    public enum PaymentResult
    {
        Pending = 1, Processed = 2, Failed = 4
    }
}