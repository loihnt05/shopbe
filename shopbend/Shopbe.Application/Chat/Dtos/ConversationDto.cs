namespace Shopbe.Application.Chat.Dtos;

public sealed record ConversationDto(
    Guid Id,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt,
    DateTime LastMessageAt,
    string? LastMessagePreview
);

