using Stripe;

namespace Shopbe.Application.Common.Interfaces;

public interface IStripeService
{
    Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(Guid orderId, decimal amount, string currency,
        CancellationToken ct);

    /// <summary>
    /// Retrieves an existing PaymentIntent from Stripe.
    /// Used to sync payment status after the client confirms payment (Stripe.js),
    /// especially in local/dev environments where webhooks may not be configured.
    /// </summary>
    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId, CancellationToken ct);

    Task<string> CreateRefundAsync(string paymentIntentId, decimal amount, CancellationToken ct);
    Event ConstructWebhookEvent(string payload, string signature);
}