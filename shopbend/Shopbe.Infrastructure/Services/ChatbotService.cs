using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Shopbe.Application.Common.Interfaces.IChat;
using Shopbe.Infrastructure.Chatbot;
using Shopbe.Infrastructure.Persistence;
using DomainChatMessage = Shopbe.Domain.Entities.Chatbot.ChatMessage;

namespace Shopbe.Infrastructure.Services;

public sealed class ChatbotService : IChatbotService
{
    private readonly ShopDbContext _db;
    private readonly ChatbotOptions _options;
    private readonly ChatClient _chat;

    public ChatbotService(ShopDbContext db, IOptions<ChatbotOptions> options)
    {
        _db = db;
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Missing Chatbot:ApiKey configuration.");

        var client = new OpenAIClient(_options.ApiKey);
        _chat = client.GetChatClient(_options.Model);
    }

    public async Task<DomainChatMessage> GenerateAssistantReplyAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _db.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

        if (conversation is null)
            throw new InvalidOperationException("Conversation not found");

        // Load last N messages to control tokens.
        var history = await _db.ChatMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(30)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        if (history.Count == 0)
            throw new InvalidOperationException("Conversation has no messages.");

        // Build system prompt. (Keep it simple; you can expand with shop placeholders later.)
        var systemPrompt = _options.SystemPrompt;

        var messages = new List<OpenAI.Chat.ChatMessage>
        {
            new SystemChatMessage(systemPrompt)
        };

        foreach (var m in history)
        {
            var sender = m.Sender.Trim().ToLowerInvariant();
            if (sender == "assistant")
                messages.Add(new AssistantChatMessage(m.Content));
            else
                messages.Add(new UserChatMessage(m.Content));
        }

        var completion = await _chat.CompleteChatAsync(messages, new ChatCompletionOptions
        {
            MaxOutputTokenCount = _options.MaxOutputTokens,
            Temperature = 0.4f
        }, cancellationToken);

        var assistantText = completion.Value.Content.Count > 0
            ? string.Concat(completion.Value.Content.Select(c => c.Text))
            : string.Empty;

        if (string.IsNullOrWhiteSpace(assistantText))
            assistantText = "Xin lỗi, mình chưa thể trả lời ngay lúc này. Bạn có thể thử hỏi lại hoặc liên hệ hỗ trợ.";

        var now = DateTime.UtcNow;
        var assistantMessage = new DomainChatMessage
        {
            ConversationId = conversationId,
            Sender = "assistant",
            Content = assistantText.Trim(),
            Metadata = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.ChatMessages.Add(assistantMessage);
        await _db.SaveChangesAsync(cancellationToken);

        return assistantMessage;
    }
}


