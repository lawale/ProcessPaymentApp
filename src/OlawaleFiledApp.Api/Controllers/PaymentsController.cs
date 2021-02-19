using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OlawaleFiledApp.Core.Models.Payloads;
using OlawaleFiledApp.Core.Models.Resources;
using OlawaleFiledApp.Core.Services;

namespace OlawaleFiledApp.Api.Controllers
{
    [Route("/[controller]")]
    public class PaymentsController : BaseController
    {
        private readonly IPaymentService paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            this.paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<ObjectResource<PaymentResource>>> ProcessPayment([FromBody] PaymentPayload payload)
        {
            var result = await paymentService.ProcessAsync(payload);
            return HandleResponse(nameof(GetPayment), result);
        }

        [HttpGet]
        public Task<ActionResult<ObjectResource<PaymentResource>>> GetPayment(Guid paymentId)
        {
            throw new NotImplementedException();
        }
    }
}