using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OlawaleFiledApp.Core.Models.Payloads;
using OlawaleFiledApp.Core.Models.Resources;
using OlawaleFiledApp.Core.Services.Payments;

namespace OlawaleFiledApp.Api.Controllers
{
    [Route("/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
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
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
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
        [ProducesResponseType(500)]
        public async Task<ActionResult<ObjectResource<PaymentResource>>> GetPayment(Guid paymentId)
        {
            throw new NotImplementedException();
        }
    }
}