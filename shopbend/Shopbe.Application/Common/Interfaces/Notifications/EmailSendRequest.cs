namespace Shopbe.Application.Common.Interfaces.Notifications;

public sealed record EmailSendRequest(
    string To,
    string Subject,
    string BodyHtml,
    string? BodyText = null);

