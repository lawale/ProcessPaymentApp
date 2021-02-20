using System;
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

            var result = await HandlePaymentTypeProcessing(payment, paymentType, payload);

            await HandlePaymentResponse(payment, result);

            return result;
        }

        public async Task<ObjectResource<PaymentResource>> GetPaymentByIdAsync(Guid paymentId)
        {
            var payment = await unitOfWork.PaymentRepository.GetByIdAsync(paymentId);

            if (payment is null)
                return new ObjectResource<PaymentResource>
                {
                    Message = "Payment Could Not Be Found",
                    ResponseType = ResponseType.NoData, Status = false
                };

            return new ObjectResource<PaymentResource>
            {
                Data = PaymentMapper.ConvertFrom(payment), Message = "Payment Successfully Retrieved",
                ResponseType = ResponseType.Success, Status = true
            };
        }

        private async Task HandlePaymentResponse(Payment payment, ObjectResource<PaymentResource> result)
        {
            await unitOfWork.CommitAsync();
            result.Data = PaymentMapper.ConvertFrom(payment);
        }

        private async Task<ObjectResource<PaymentResource>> HandlePaymentTypeProcessing(Payment payment, PaymentType paymentType,
            PaymentPayload payload)
        {
            try
            {
                var paymentGateway = paymentGatewayFactory.ResolveGateway(paymentType);

                if (paymentType == PaymentType.Expensive && paymentGateway is null)
                {
                    await AddPaymentState(payment.Id, paymentType, false, "Could Not Find Expensive Gateway");
                    paymentGateway = paymentGatewayFactory.ResolveGateway(PaymentType.Cheap);
                }

                if (paymentGateway is not null)
                    return await HandlePaymentProcessing(payment, paymentGateway, payload);

                await AddPaymentState(payment.Id, paymentType, false, $"Could Not Find {paymentType} Gateway");

            }
            catch (PaymentGatewayException e)
            {
                logger.LogCritical(e, "No Gateway was found");
                await AddPaymentState(payment.Id, paymentType, false, $"Could Not Resolve Any Gateway");
            }
            
            return new ObjectResource<PaymentResource>
            {
                Status = false,
                Message = "No Payment Gateway available for processing payments",
                ResponseType = ResponseType.ServiceError
            };
        }

        private async Task<ObjectResource<PaymentResource>> HandlePaymentProcessing(Payment payment, IPaymentGateway paymentGateway,
            PaymentPayload payload)
        {
            var result = await paymentGateway.ChargeCardAsync(payload);

            var isSuccessful = result.HasValue && result.Value;
            var errorMessage = result.HasValue ? "Could Not Debit Card" : "Service Error Occurred On Gateway";
            
            await AddPaymentState(payment.Id, paymentGateway.Type, isSuccessful,
                isSuccessful ? string.Empty : errorMessage);

            if (paymentGateway.Type == PaymentType.Expensive && (!result.HasValue || !result.Value))
                result = await HandleRetryOnCheapGateway(payment, payload);

            if (result.HasValue && result.Value)
            {
                payment.State = PaymentResult.Processed;
                logger.LogInformation("Successfully processed payment");
                return new ObjectResource<PaymentResource>
                {
                    Message = "Payment Successfully processed", ResponseType = ResponseType.Created,
                    Status = result.Value
                };
            }

            payment.State = result.HasValue ? PaymentResult.Failed : PaymentResult.Pending;
            
            logger.LogInformation("could not process payment");
            return new ObjectResource<PaymentResource>
            {
                Message = "Payment Could Not Be Processed", ResponseType = ResponseType.ServiceError,
                Status = result.HasValue
            };
        }

        private async Task<bool> HandleRetryOnCheapGateway(Payment payment, PaymentPayload payload)
        {
            var result = await HandlePaymentTypeProcessing(payment, PaymentType.Cheap, payload);

            if (!result.Status)
            {
                logger.LogError("Could Not Process Payment with Either Cheap Or Expensive Gateway");
            }

            return result.Status;
        }

        //Add payment state after each card charge try
        private async Task AddPaymentState(Guid paymentId, PaymentType paymentType, bool successfulPayment, string reason)
        {
            await unitOfWork.PaymentStateRepository.CreateNewAsync(new PaymentState
            {
                PaymentId = paymentId, PaymentGatewayType = paymentType.ToString(), Reason = reason,
                PaymentResult = successfulPayment ? PaymentResult.Processed : PaymentResult.Failed
            });
        }
        
        private static PaymentType DerivePaymentType(PaymentPayload payload)
        {
            if (payload.Amount <= 20)
                return PaymentType.Cheap;
            
            return payload.Amount > 500 ? PaymentType.Premium : PaymentType.Expensive;
        }
    }
}