using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.IChat;
using Shopbe.Infrastructure.Chatbot;
using Shopbe.Infrastructure.Persistence;
using DomainChatMessage = Shopbe.Domain.Entities.Chatbot.ChatMessage;
using System.Text.Json;

namespace Shopbe.Infrastructure.Services;

public sealed class ChatbotService : IChatbotService
{
    private readonly ShopDbContext _db;
    private readonly ChatbotOptions _options;
    private readonly IRecommendationService _recommendations;
    private readonly ICurrentUser _currentUser;
    private readonly ChatClient _chat;

    public ChatbotService(
        ShopDbContext db, 
        IOptions<ChatbotOptions> options,
        IRecommendationService recommendations,
        ICurrentUser currentUser)
    {
        _db = db;
        _options = options.Value;
        _recommendations = recommendations;
        _currentUser = currentUser;

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

        // Define tools
        var getRecommendationsTool = ChatTool.CreateFunctionTool(
            functionName: "get_product_recommendations",
            functionDescription: "Get personalized product recommendations for the current user based on their behavior (clicks, cart, purchases). Use this whenever the user asks for recommendations, suggestions, or what's new."
        );

        // Build system prompt using the user's high-quality formatting instructions.
        var systemPrompt = @"
You are an e-commerce recommendation response formatter for Shopbee.
Your primary goal is to help users discover products they love.

### PHASE 1: DATA GATHERING
When a user asks for recommendations, you MUST first use the 'get_product_recommendations' tool to fetch real data.

### PHASE 2: FORMATTING
Once you have the data, transform the raw JSON into a clean, human-friendly shopping recommendation following these requirements:
- NEVER display raw JSON in your final answer.
- Convert the response into a readable recommendation card/list using markdown.
- Start with a short personalized recommendation summary based on the reasoning.
- Group products naturally if categories repeat.
- For each product show:
  - Product name
  - Category
  - Price (formatted cleanly)
- Remove technical fields like 'id'.
- Add small natural language descriptions instead of repeating category names.
- Keep output concise and modern like Shopee/Lazada/Amazon recommendations.
- Add a final CTA encouraging user interaction.

Output format template:

✨ Recommended for you

[recommendation_reason rewritten naturally]

🛍 Product Recommendations

1. Product Name
Category: ...
Price: ...
Why this may fit:
...

2. Product Name
Category: ...
Price: ...
Why this may fit:
...

👉 Want more like these?
You can:
• View similar products
• Filter by category
• Get recommendations based on your recent activity

Rules:
- Use natural language. Avoid robotic phrases.
- Do not mention missing fields.
- Make recommendations feel personalized.
- If the tool returns no products -> explain politely and suggest exploring categories.
";

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

        var chatOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _options.MaxOutputTokens,
            Temperature = 0.2f,
            Tools = { getRecommendationsTool }
        };

        ChatCompletion completion;
        bool requiresAction;

        do
        {
            requiresAction = false;
            completion = await _chat.CompleteChatAsync(messages, chatOptions, cancellationToken);

            if (completion.FinishReason == ChatFinishReason.ToolCalls)
            {
                requiresAction = true;
                messages.Add(new AssistantChatMessage(completion));

                foreach (var toolCall in completion.ToolCalls)
                {
                    if (toolCall.FunctionName == "get_product_recommendations")
                    {
                        // Resolve current user to provide personalized data
                        var keycloakId = _currentUser.KeycloakId;
                        var user = await _db.Users
                            .AsNoTracking()
                            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId, cancellationToken);

                        List<Shopbe.Application.Product.Products.Dtos.ProductResponseDto> results;
                        if (user != null)
                        {
                            results = await _recommendations.GetPersonalizedAsync(user.Id, 10);
                        }
                        else
                        {
                            results = await _recommendations.GetTopSellingAsync(10);
                        }

                        var toolResult = JsonSerializer.Serialize(results);
                        messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                    }
                }
            }
        } while (requiresAction);

        var assistantText = completion.Content.Count > 0
            ? string.Concat(completion.Content.Select(c => c.Text))
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


