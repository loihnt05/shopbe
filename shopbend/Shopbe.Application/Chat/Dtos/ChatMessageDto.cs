using System.Text.Json;

namespace Shopbe.Application.Chat.Dtos;

public sealed record ChatMessageDto(
    Guid Id,
    Guid ConversationId,
    string Sender,
    string Content,
    JsonDocument? Metadata,
    DateTime CreatedAt
);

