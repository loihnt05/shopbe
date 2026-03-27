namespace Shopbe.Domain.Entities.Chatbot;

public class Conversation : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;

    // Navigation properties
    public User.User? User { get; set; }
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}

public class ChatMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    // Navigation properties
    public Conversation? Conversation { get; set; }
}

