using System.Threading.Tasks;
using OlawaleFiledApp.Core.Models.Payloads;

namespace OlawaleFiledApp.Core.Services.Payments.Gateways
{
    public interface IPaymentGateway : IPaymentType
    {
        Task<bool> ChargeCardAsync(PaymentPayload payload);
    }
}