using System.Text.Json;

namespace Shopbe.Domain.Entities.Chatbot;

public class ChatMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public JsonDocument? Metadata { get; set; }

    // Navigation Properties
    public Conversation? Conversation { get; set; }
}

