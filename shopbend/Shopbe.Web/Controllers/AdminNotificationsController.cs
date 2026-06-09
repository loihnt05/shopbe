using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.Notifications;
using Shopbe.Domain.Entities.Notification;
using Shopbe.Domain.Enums;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = "Admin")]
public sealed class AdminNotificationsController(ShopDbContext dbContext, IEmailQueue emailQueue) : ControllerBase
{
    public sealed class SendMarketingNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? EmailSubject { get; set; }
    }

    public sealed record NotificationActionResultDto(int InAppNotificationsCreated, int EmailsQueued);
    public sealed record LowStockNotificationResultDto(int ProductsMatched, int AdminNotificationsCreated);

    [HttpPost("marketing")]
    public async Task<ActionResult<NotificationActionResultDto>> SendMarketingNotification(
        [FromBody] SendMarketingNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();
        var message = request.Message.Trim();
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
        {
            return BadRequest("Title and message are required.");
        }

        var recipients = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Customer && u.DeletedAt == null)
            .GroupJoin(
                dbContext.NotificationPreferences.AsNoTracking(),
                user => user.Id,
                preference => preference.UserId,
                (user, preferences) => new
                {
                    User = user,
                    Preferences = preferences.FirstOrDefault()
                })
            .Select(x => new
            {
                x.User,
                MarketingEmailsEnabled = x.Preferences != null && x.Preferences.MarketingEmailsEnabled,
                InAppNotificationsEnabled = x.Preferences == null || x.Preferences.InAppNotificationsEnabled
            })
            .Where(x => x.MarketingEmailsEnabled || x.InAppNotificationsEnabled)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var recipient in recipients.Where(x => x.InAppNotificationsEnabled))
        {
            dbContext.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = recipient.User.Id,
                Channel = "InApp",
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var emailSubject = string.IsNullOrWhiteSpace(request.EmailSubject)
            ? title
            : request.EmailSubject.Trim();
        var emailsQueued = 0;

        foreach (var recipient in recipients.Where(x => x.MarketingEmailsEnabled))
        {
            await emailQueue.EnqueueAsync(
                recipient.User.Email,
                emailSubject,
                BuildMarketingEmailHtml(title, message),
                message,
                recipient.User.Id,
                null,
                null,
                $"marketing:{title}:{recipient.User.Id}:{now:yyyyMMddHHmmss}",
                cancellationToken);
            emailsQueued++;
        }

        return Ok(new NotificationActionResultDto(
            recipients.Count(x => x.InAppNotificationsEnabled),
            emailsQueued));
    }

    [HttpPost("low-stock")]
    public async Task<ActionResult<LowStockNotificationResultDto>> CreateLowStockAlerts(
        [FromQuery] int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        threshold = Math.Max(0, threshold);

        var lowStockProducts = await dbContext.Products
            .AsNoTracking()
            .Where(p => p.DeletedAt == null && p.IsActive)
            .Select(p => new
            {
                p.Id,
                p.Name,
                Stock = p.Variants
                    .Where(v => v.DeletedAt == null && v.IsActive)
                    .Sum(v => v.StockQuantity)
            })
            .Where(p => p.Stock <= threshold)
            .OrderBy(p => p.Stock)
            .Take(50)
            .ToListAsync(cancellationToken);

        var admins = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Admin && u.DeletedAt == null)
            .GroupJoin(
                dbContext.NotificationPreferences.AsNoTracking(),
                user => user.Id,
                preference => preference.UserId,
                (user, preferences) => new
                {
                    User = user,
                    Preferences = preferences.FirstOrDefault()
                })
            .Where(x => x.Preferences == null || x.Preferences.InAppNotificationsEnabled)
            .Select(x => x.User)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var admin in admins)
        {
            foreach (var product in lowStockProducts)
            {
                dbContext.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = admin.Id,
                    Channel = "InApp",
                    Title = "Low stock alert",
                    Message = $"{product.Name} has {product.Stock} units left.",
                    IsRead = false,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new LowStockNotificationResultDto(
            lowStockProducts.Count,
            lowStockProducts.Count * admins.Count));
    }

    private static string BuildMarketingEmailHtml(string title, string message)
    {
        return $"""
            <!doctype html>
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.5; color: #111827;">
              <h2>{WebUtility.HtmlEncode(title)}</h2>
              <p>{WebUtility.HtmlEncode(message)}</p>
              <p>You are receiving this because marketing emails are enabled in your Shopbe notification preferences.</p>
            </body>
            </html>
            """;
    }
}
