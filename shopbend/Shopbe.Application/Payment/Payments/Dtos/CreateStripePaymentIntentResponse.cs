namespace Shopbe.Application.Payment.Payments.Dtos;

public sealed class CreateStripePaymentIntentResponse
{
    public Guid PaymentId { get; set; }
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
}