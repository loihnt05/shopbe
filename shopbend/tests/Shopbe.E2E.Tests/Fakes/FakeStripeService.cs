using System.Text.Json;
using Shopbe.Application.Common.Interfaces;
using Stripe;

namespace Shopbe.E2E.Tests.Fakes;

/// <summary>
/// Deterministic Stripe implementation for tests.
/// - CreatePaymentIntentAsync returns a predictable intent id and client secret.
/// - ConstructWebhookEvent bypasses signature verification (tests focus on app logic).
/// </summary>
public sealed class FakeStripeService : IStripeService
{
    public Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(
        Guid orderId,
        decimal amount,
        string currency,
        CancellationToken ct)
    {
        var id = $"pi_test_{orderId:N}";
        var secret = $"cs_test_{orderId:N}";
        return Task.FromResult((id, secret));
    }

    public Task<string> CreateRefundAsync(string paymentIntentId, decimal amount, CancellationToken ct)
        => Task.FromResult($"re_test_{paymentIntentId}");

    public Event ConstructWebhookEvent(string payload, string signature)
    {
        // Minimal emulation: Stripe's SDK returns Event with Data.Object typed to PaymentIntent, etc.
        // We'll parse enough to support PaymentsController logic.
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var evt = new Event
        {
            Id = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : Guid.NewGuid().ToString("N"),
            Type = root.TryGetProperty("type", out var typeEl) ? typeEl.GetString() : null
        };

        if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Object &&
            dataEl.TryGetProperty("object", out var objEl) && objEl.ValueKind == JsonValueKind.Object)
        {
            // Expect { id: "pi_..." }
            var intent = new PaymentIntent
            {
                Id = objEl.TryGetProperty("id", out var objIdEl) ? objIdEl.GetString() : null
            };

            evt.Data = new EventData { Object = intent };
        }

        return evt;
    }
}

