using System.Text.Json;

namespace Shopbe.Application.Chat.Dtos;

public sealed record SendMessageRequestDto(
    string Content,
    JsonDocument? Metadata
);

