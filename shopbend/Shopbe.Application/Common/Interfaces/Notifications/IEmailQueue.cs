namespace Shopbe.Application.Common.Interfaces.Notifications;

public interface IEmailQueue
{
    Task<Guid> EnqueueAsync(
        string to,
        string subject,
        string bodyHtml,
        string? bodyText = null,
        Guid? userId = null,
        Guid? orderId = null,
        Guid? paymentId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);
}

