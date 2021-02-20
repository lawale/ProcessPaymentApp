using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OlawaleFiledApp.Core.Data.Repositories;
using OlawaleFiledApp.Core.Domain;
using OlawaleFiledApp.Core.Models.Payloads;
using OlawaleFiledApp.Core.Models.Resources;
using OlawaleFiledApp.Core.Services.Payments.Gateways;
using OlawaleFiledApp.Core.Services.Payments.Gateways.Exceptions;
using OlawaleFiledApp.Core.Services.Payments.Gateways.Factory;

namespace OlawaleFiledApp.Core.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly ILogger<PaymentService> logger;
        private readonly IUnitOfWork unitOfWork;
        private readonly IPaymentGatewayFactory paymentGatewayFactory;

        public PaymentService(ILogger<PaymentService> logger, IUnitOfWork unitOfWork, IPaymentGatewayFactory paymentGatewayFactory)
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
            this.paymentGatewayFactory = paymentGatewayFactory;
        }
        
        public async Task<ObjectResource<PaymentResource>> ProcessAsync(PaymentPayload payload)
        {
            logger.LogInformation("Starting Unit Of Work For Payment Processing");
            await unitOfWork.StartAsync();

            var payment = PaymentMapper.ConvertFrom(payload);

            await unitOfWork.PaymentRepository.CreateNewAsync(payment);

            var paymentType = DerivePaymentType(payload);
            logger.LogInformation("Derived Payment Type of {0} for {1}", paymentType, payload.Amount);

            var result = await HandlePaymentTypeProcessing(paymentType, payload);

            await HandlePaymentResponse(payment, result);

            return result;
        }

        private async Task HandlePaymentResponse(Payment payment, ObjectResource<PaymentResource> result)
        {
            payment.State = result.Status ? PaymentState.Processed : PaymentState.Failed;
            await unitOfWork.PaymentRepository.UpdateAsync(payment);

            if (result.Status)
            {
                await unitOfWork.CommitAsync();
                result.Data = PaymentMapper.ConvertFrom(payment);
            }
            else
                await unitOfWork.RollbackAsync();
        }

        private async Task<ObjectResource<PaymentResource>> HandlePaymentTypeProcessing(PaymentType paymentType,
            PaymentPayload payload)
        {
            try
            {
                var paymentGateway = paymentGatewayFactory.ResolveGateway(paymentType);

                if (paymentType == PaymentType.Expensive && paymentGateway is null)
                    paymentGateway = paymentGatewayFactory.ResolveGateway(PaymentType.Cheap);

                if (paymentGateway is not null)
                    return await HandlePaymentProcessing(paymentGateway, payload);

            }
            catch (PaymentGatewayException e)
            {
                logger.LogCritical(e, "No Gateway was found");
            }
            return new ObjectResource<PaymentResource>
            {
                Status = false,
                Message = "No Payment Gateway available for processing payments",
                ResponseType = ResponseType.ServiceError
            };
        }

        private async Task<ObjectResource<PaymentResource>> HandlePaymentProcessing(IPaymentGateway paymentGateway,
            PaymentPayload payload)
        {
            var result = await paymentGateway.ChargeCardAsync(payload);

            if (paymentGateway.Type == PaymentType.Expensive && !result)
                result = await HandleRetryOnCheapGateway(payload);

            if (result)
            {
                logger.LogInformation("Successfully processed payment");
                return new ObjectResource<PaymentResource>
                {
                    Message = "Payment Successfully processed", ResponseType = ResponseType.Created,
                    Status = result
                };
            }
            
            logger.LogInformation("could not process payment");
            return new ObjectResource<PaymentResource>
            {
                Message = "Payment Could Not Be Processed", ResponseType = ResponseType.ServiceError,
                Status = result
            };
        }

        private async Task<bool> HandleRetryOnCheapGateway(PaymentPayload payload)
        {
            var result = await HandlePaymentTypeProcessing(PaymentType.Cheap, payload);

            logger.LogError("Could Not Process Payment with Either Cheap Or Expensive Gateway");

            return result.Status;
        }
        
        private PaymentType DerivePaymentType(PaymentPayload payload)
        {
            if (payload.Amount <= 20)
                return PaymentType.Cheap;
            
            return payload.Amount > 500 ? PaymentType.Premium : PaymentType.Expensive;
        }
    }
}