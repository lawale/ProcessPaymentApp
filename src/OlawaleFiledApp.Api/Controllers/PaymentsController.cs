using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OlawaleFiledApp.Core.Models.Payloads;
using OlawaleFiledApp.Core.Models.Resources;
using OlawaleFiledApp.Core.Services;
using OlawaleFiledApp.Core.Services.Payments;

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

        /// <summary>
        /// Processes a payment
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<ObjectResource<PaymentResource>>> ProcessPayment([FromBody] PaymentPayload payload)
        {
            var result = await paymentService.ProcessAsync(payload);
            return HandleResponse(nameof(GetPayment), result);
        }

        /// <summary>
        /// Retrieves A Payment
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("{paymentId}")]
        public Task<ActionResult<ObjectResource<PaymentResource>>> GetPayment(Guid paymentId)
        {
            throw new NotImplementedException();
        }
    }
}