namespace OlawaleFiledApp.Core.Services.Payments.Gateways.Factory
{
    public interface IPaymentGatewayFactory
    {
        /// <summary>
        /// Resolves a payment gateway of the specified payment type
        /// </summary>
        /// <param name="paymentType"></param>
        /// <returns></returns>
        /// <exception cref="OlawaleFiledApp.Core.Services.Payments.Gateways.Exceptions.PaymentGatewayException">Thrown If No Gateway was found</exception>
        IPaymentGateway? ResolveGateway(PaymentType paymentType);
    }
}