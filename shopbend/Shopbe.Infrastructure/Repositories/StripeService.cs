// Infrastructure/Services/StripeService.cs

using Microsoft.Extensions.Options;
using Shopbe.Application.Common.Interfaces;
using Stripe;

namespace Shopbe.Infrastructure.Repositories;

public class StripeService(IOptions<Shopbe.Infrastructure.StripeOptions> opts) : IStripeService
{
    private static long ToStripeAmount(decimal amount, string currency)
    {
        // Stripe expects the amount in the smallest currency unit.
        // Some currencies are "zero-decimal" (e.g., VND, JPY) where the smallest unit is 1.
        // See: https://stripe.com/docs/currencies#zero-decimal
        var zeroDecimalCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA", "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF"
        };

        if (zeroDecimalCurrencies.Contains(currency))
            return (long)decimal.Round(amount, 0, MidpointRounding.AwayFromZero);

        return (long)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
    }

    public async Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(
        Guid orderId, decimal amount, string currency, CancellationToken ct)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount             = ToStripeAmount(amount, currency),
            Currency           = currency.ToLower(),
            Metadata           = new Dictionary<string, string> { ["order_id"] = orderId.ToString() },
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                // Avoid redirect-based payment methods (which require return_url on confirmation).
                // Keeps the flow compatible with backend/CLI confirmation for card payments.
                AllowRedirects = "never"
            }
        };

        var service = new PaymentIntentService();
        var intent  = await service.CreateAsync(options, cancellationToken: ct);
        return (intent.Id, intent.ClientSecret);  // trả về frontend
    }

    public async Task<string> CreateRefundAsync(
        string paymentIntentId, decimal amount, CancellationToken ct)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount        = ToStripeAmount(amount, "VND"),
        };

        var service = new RefundService();
        var refund  = await service.CreateAsync(options, cancellationToken: ct);
        return refund.Id;
    }

    public Event ConstructWebhookEvent(string payload, string signature)
        => EventUtility.ConstructEvent(payload, signature, opts.Value.WebhookSecret);
}