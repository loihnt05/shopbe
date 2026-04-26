using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopbe.Application.Common.Interfaces.Notifications;
using Shopbe.Domain.Entities.Notification;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Services.Email;

public sealed class EmailProcessorJob(
    ShopDbContext db,
    IEmailSender emailSender,
    ILogger<EmailProcessorJob> logger)
{
    [AutomaticRetry(Attempts = 0)] // we handle attempts ourselves + persist status
    public async Task ProcessAsync(Guid emailMessageId, CancellationToken cancellationToken = default)
    {
        var msg = await db.EmailMessages
            .FirstOrDefaultAsync(x => x.Id == emailMessageId, cancellationToken);

        if (msg is null)
        {
            logger.LogWarning("EmailMessage {EmailMessageId} not found", emailMessageId);
            return;
        }

        if (msg.Status == EmailDeliveryStatus.Sent)
        {
            return; // idempotent
        }

        if (msg.Status == EmailDeliveryStatus.DeadLetter)
        {
            return;
        }

        if (msg.AttemptCount >= msg.MaxAttempts)
        {
            msg.Status = EmailDeliveryStatus.DeadLetter;
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        msg.AttemptCount += 1;
        msg.LastAttemptAt = DateTime.UtcNow;

        try
        {
            await emailSender.SendAsync(new EmailSendRequest(msg.To, msg.Subject, msg.BodyHtml, msg.BodyText), cancellationToken);
            msg.Status = EmailDeliveryStatus.Sent;
            msg.SentAt = DateTime.UtcNow;
            msg.LastError = null;
        }
        catch (Exception ex)
        {
            msg.Status = msg.AttemptCount >= msg.MaxAttempts
                ? EmailDeliveryStatus.DeadLetter
                : EmailDeliveryStatus.Failed;
            msg.LastError = ex.ToString();
            logger.LogError(ex, "Sending EmailMessage {EmailMessageId} failed (attempt {Attempt}/{Max})", msg.Id, msg.AttemptCount, msg.MaxAttempts);
            throw; // allow Hangfire to mark job failed for visibility
        }
        finally
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}

