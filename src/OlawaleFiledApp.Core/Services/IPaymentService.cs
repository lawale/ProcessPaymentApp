using System.Threading.Tasks;
using OlawaleFiledApp.Core.Models.Payloads;
using OlawaleFiledApp.Core.Models.Resources;

namespace OlawaleFiledApp.Core.Services
{
    public interface IPaymentService
    {
        Task<ObjectResource<PaymentResource>> ProcessAsync(PaymentPayload payload);
    }
}