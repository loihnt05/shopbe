namespace Shopbe.Application.Common.Interfaces.Notifications;

public interface INotificationService
{
    Task SendOrderPlacedAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task SendPaymentSucceededAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task SendPaymentFailedAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task SendOrderShippedAsync(Guid orderId, string? carrier = null, string? trackingNumber = null, CancellationToken cancellationToken = default);
    Task SendOrderDeliveredAsync(Guid orderId, string? carrier = null, string? trackingNumber = null, CancellationToken cancellationToken = default);
}
