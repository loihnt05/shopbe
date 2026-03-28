namespace Shopbe.Domain.Entities.Chatbot;

public class Conversation : BaseEntity
{
    public Guid UserId { get; set; }
    public string Status { get; set; } = "active";
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    // Navigation properties
    public User.User? User { get; set; }
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
