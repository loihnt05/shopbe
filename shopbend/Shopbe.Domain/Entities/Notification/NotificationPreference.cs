namespace Shopbe.Domain.Entities.Notification;

public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public bool OrderStatusEmailsEnabled { get; set; } = true;
    public bool PaymentEmailsEnabled { get; set; } = true;
    public bool MarketingEmailsEnabled { get; set; }
    public bool InAppNotificationsEnabled { get; set; } = true;

    public User.User? User { get; set; }
}
