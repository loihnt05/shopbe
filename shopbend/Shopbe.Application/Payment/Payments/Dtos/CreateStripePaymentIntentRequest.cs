namespace Shopbe.Application.Payment.Payments.Dtos;

public sealed class CreateStripePaymentIntentRequest
{
    public Guid OrderId { get; set; }
}