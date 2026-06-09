using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Domain.Entities.Notification;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    ShopDbContext dbContext) : ControllerBase
{
    public sealed record NotificationDto(
        Guid Id,
        string Channel,
        string Type,
        string Title,
        string Message,
        string? LinkUrl,
        bool IsRead,
        DateTime? ReadAt,
        DateTime CreatedAt);

    public sealed record NotificationPageDto(
        IReadOnlyList<NotificationDto> Items,
        int Page,
        int PageSize,
        long TotalCount);

    public sealed record UnreadCountDto(long Count);
    public sealed record NotificationPreferenceDto(
        bool OrderStatusEmailsEnabled,
        bool PaymentEmailsEnabled,
        bool MarketingEmailsEnabled,
        bool InAppNotificationsEnabled);

    private async Task<Guid> GetAppUserIdAsync(CancellationToken cancellationToken)
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            throw new UnauthorizedAccessException("Missing user identity");
        }

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        if (user is null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        return user.Id;
    }

    [HttpGet]
    public async Task<ActionResult<NotificationPageDto>> GetMyNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderBy(n => n.IsRead)
            .ThenByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto(
                n.Id,
                n.Channel,
                n.Type,
                n.Title,
                n.Message,
                n.LinkUrl,
                n.IsRead,
                n.ReadAt,
                n.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(new NotificationPageDto(items, page, pageSize, totalCount));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var count = await dbContext.Notifications
            .AsNoTracking()
            .LongCountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

        return Ok(new UnreadCountDto(count));
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferenceDto>> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var preferences = await GetOrCreatePreferencesAsync(userId, cancellationToken);
        return Ok(ToDto(preferences));
    }

    [HttpPut("preferences")]
    public async Task<ActionResult<NotificationPreferenceDto>> UpdatePreferences(
        [FromBody] NotificationPreferenceDto request,
        CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var preferences = await GetOrCreatePreferencesAsync(userId, cancellationToken);

        preferences.OrderStatusEmailsEnabled = request.OrderStatusEmailsEnabled;
        preferences.PaymentEmailsEnabled = request.PaymentEmailsEnabled;
        preferences.MarketingEmailsEnabled = request.MarketingEmailsEnabled;
        preferences.InAppNotificationsEnabled = request.InAppNotificationsEnabled;
        preferences.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(preferences));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);

        if (notification is null)
        {
            return NotFound();
        }

        if (!notification.IsRead)
        {
            var now = DateTime.UtcNow;
            notification.IsRead = true;
            notification.ReadAt = now;
            notification.UpdatedAt = now;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var now = DateTime.UtcNow;
        await dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, now)
                    .SetProperty(n => n.UpdatedAt, now),
                cancellationToken);

        return NoContent();
    }

    private async Task<NotificationPreference> GetOrCreatePreferencesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var preferences = await dbContext.NotificationPreferences
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (preferences is not null)
        {
            return preferences;
        }

        preferences = new NotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderStatusEmailsEnabled = true,
            PaymentEmailsEnabled = true,
            MarketingEmailsEnabled = false,
            InAppNotificationsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.NotificationPreferences.Add(preferences);
        await dbContext.SaveChangesAsync(cancellationToken);

        return preferences;
    }

    private static NotificationPreferenceDto ToDto(NotificationPreference preferences)
    {
        return new NotificationPreferenceDto(
            preferences.OrderStatusEmailsEnabled,
            preferences.PaymentEmailsEnabled,
            preferences.MarketingEmailsEnabled,
            preferences.InAppNotificationsEnabled);
    }
}
