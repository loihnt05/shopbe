using Stripe;

namespace Shopbe.Application.Common.Interfaces;

public interface IStripeService
{
    Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(Guid orderId, decimal amount, string currency,
        CancellationToken ct);
    Task<string> CreateRefundAsync(string paymentIntentId, decimal amount, CancellationToken ct);
    Event ConstructWebhookEvent(string payload, string signature);
}