namespace Shopbe.Application.Common.Interfaces.Notifications;

public interface IEmailSender
{
    Task SendAsync(EmailSendRequest request, CancellationToken cancellationToken = default);
}

