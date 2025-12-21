using ReSys.Shop.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Shop.Core.Domain.Orders.Payments.Gateways;

public sealed class PaymentProcessorFactory(IEnumerable<IPaymentProcessor> processors)
{
    public ErrorOr<IPaymentProcessor> GetProcessor(PaymentMethod.PaymentType type)
    {
        var processor = processors.FirstOrDefault(p => p.Type == type);
        
        if (processor == null)
            return Error.Validation("Payment.ProcessorNotFound", $"No implementation for {type}");

        return processor.ToErrorOr();
    }
}
