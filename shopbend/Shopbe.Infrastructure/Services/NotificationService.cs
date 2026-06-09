using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shopbe.Application.Common.Interfaces.Notifications;
using Shopbe.Domain.Entities.Notification;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Services;

public sealed class NotificationService(
    ShopDbContext db,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationService> logger) : INotificationService
{
    public Task SendOrderPlacedAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return SendOrderEventAsync(
            orderId,
            "Order placed",
            "Your order has been placed and is waiting for payment confirmation.",
            "Your Shopbe order has been placed",
            "order-placed",
            cancellationToken);
    }

    public async Task SendPaymentSucceededAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await db.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                .ThenInclude(o => o!.User)
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

            if (payment?.Order?.User is null)
            {
                logger.LogWarning("Payment success notification skipped. Payment {PaymentId} or order user was not found.", paymentId);
                return;
            }

            var order = payment.Order;
            var user = order.User;
            var amount = FormatMoney(payment.Amount, payment.Currency);
            var message = $"Payment for order #{ShortId(order.Id)} was successful.";
            var preferences = await GetPreferencesAsync(user.Id, cancellationToken);

            if (preferences.InAppNotificationsEnabled)
            {
                await CreateInAppAsync(user.Id, "Payment successful", message, cancellationToken);
            }

            if (preferences.PaymentEmailsEnabled)
            {
                await EnqueueEmailAsync(
                    user.Email,
                    "Payment received for your Shopbe order",
                    BuildEmailHtml("Payment successful", $"We received your payment of {amount} for order #{ShortId(order.Id)}.", order.Id),
                    $"We received your payment of {amount} for order #{ShortId(order.Id)}.",
                    user.Id,
                    order.Id,
                    payment.Id,
                    $"payment-succeeded:{payment.Id}",
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send payment success notification for payment {PaymentId}.", paymentId);
        }
    }

    public async Task SendPaymentFailedAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await db.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                .ThenInclude(o => o!.User)
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

            if (payment?.Order?.User is null)
            {
                logger.LogWarning("Payment failure notification skipped. Payment {PaymentId} or order user was not found.", paymentId);
                return;
            }

            var order = payment.Order;
            var user = order.User;
            var message = $"Payment for order #{ShortId(order.Id)} failed. Please try another payment method.";
            var preferences = await GetPreferencesAsync(user.Id, cancellationToken);

            if (preferences.InAppNotificationsEnabled)
            {
                await CreateInAppAsync(user.Id, "Payment failed", message, cancellationToken);
            }

            if (preferences.PaymentEmailsEnabled)
            {
                await EnqueueEmailAsync(
                    user.Email,
                    "Payment failed for your Shopbe order",
                    BuildEmailHtml("Payment failed", message, order.Id),
                    message,
                    user.Id,
                    order.Id,
                    payment.Id,
                    $"payment-failed:{payment.Id}",
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send payment failure notification for payment {PaymentId}.", paymentId);
        }
    }

    public Task SendOrderShippedAsync(Guid orderId, string? carrier = null, string? trackingNumber = null, CancellationToken cancellationToken = default)
    {
        var tracking = BuildTrackingText(carrier, trackingNumber);
        return SendOrderEventAsync(
            orderId,
            "Order shipped",
            $"Your order #{ShortId(orderId)} has shipped.{tracking}",
            "Your Shopbe order has shipped",
            "order-shipped",
            cancellationToken);
    }

    public Task SendOrderDeliveredAsync(Guid orderId, string? carrier = null, string? trackingNumber = null, CancellationToken cancellationToken = default)
    {
        var tracking = BuildTrackingText(carrier, trackingNumber);
        return SendOrderEventAsync(
            orderId,
            "Order delivered",
            $"Your order #{ShortId(orderId)} has been delivered.{tracking}",
            "Your Shopbe order was delivered",
            "order-delivered",
            cancellationToken);
    }

    private async Task SendOrderEventAsync(
        Guid orderId,
        string title,
        string message,
        string subject,
        string idempotencyPrefix,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await db.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order?.User is null)
            {
                logger.LogWarning("{NotificationTitle} notification skipped. Order {OrderId} or user was not found.", title, orderId);
                return;
            }

            var preferences = await GetPreferencesAsync(order.UserId, cancellationToken);

            if (preferences.InAppNotificationsEnabled)
            {
                await CreateInAppAsync(order.UserId, title, message, cancellationToken);
            }

            if (preferences.OrderStatusEmailsEnabled)
            {
                await EnqueueEmailAsync(
                    order.User.Email,
                    subject,
                    BuildEmailHtml(title, message, order.Id),
                    message,
                    order.UserId,
                    order.Id,
                    null,
                    $"{idempotencyPrefix}:{order.Id}",
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send {NotificationTitle} notification for order {OrderId}.", title, orderId);
        }
    }

    private async Task<NotificationPreferenceSnapshot> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var preferences = await db.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        return preferences is null
            ? NotificationPreferenceSnapshot.Default
            : new NotificationPreferenceSnapshot(
                preferences.OrderStatusEmailsEnabled,
                preferences.PaymentEmailsEnabled,
                preferences.InAppNotificationsEnabled);
    }

    private async Task EnqueueEmailAsync(
        string to,
        string subject,
        string bodyHtml,
        string? bodyText,
        Guid? userId,
        Guid? orderId,
        Guid? paymentId,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var emailQueue = scope.ServiceProvider.GetRequiredService<IEmailQueue>();
        await emailQueue.EnqueueAsync(to, subject, bodyHtml, bodyText, userId, orderId, paymentId, idempotencyKey, cancellationToken);
    }

    private async Task CreateInAppAsync(Guid userId, string title, string message, CancellationToken cancellationToken)
    {
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Channel = "InApp",
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string BuildEmailHtml(string title, string message, Guid orderId)
    {
        var encodedTitle = WebUtility.HtmlEncode(title);
        var encodedMessage = WebUtility.HtmlEncode(message);
        return $"""
            <!doctype html>
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.5; color: #111827;">
              <h2>{encodedTitle}</h2>
              <p>{encodedMessage}</p>
              <p>Order ID: {orderId}</p>
              <p>Thank you for shopping with Shopbe.</p>
            </body>
            </html>
            """;
    }

    private static string BuildTrackingText(string? carrier, string? trackingNumber)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(carrier)) parts.Add($"carrier: {carrier.Trim()}");
        if (!string.IsNullOrWhiteSpace(trackingNumber)) parts.Add($"tracking: {trackingNumber.Trim()}");

        return parts.Count == 0 ? string.Empty : " " + string.Join(", ", parts) + ".";
    }

    private static string FormatMoney(decimal amount, string currency)
    {
        return $"{amount:N0} {currency}";
    }

    private static string ShortId(Guid id)
    {
        return id.ToString("N")[..8].ToUpperInvariant();
    }

    private sealed record NotificationPreferenceSnapshot(
        bool OrderStatusEmailsEnabled,
        bool PaymentEmailsEnabled,
        bool InAppNotificationsEnabled)
    {
        public static NotificationPreferenceSnapshot Default { get; } = new(true, true, true);
    }
}
