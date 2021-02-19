using System;

namespace OlawaleFiledApp.Core.Domain
{
    [Flags]
    public enum PaymentState
    {
        Pending = 1, Processed = 2, Failed = 4
    }
}