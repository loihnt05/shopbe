namespace Shopbe.Infrastructure.Chatbot;

public sealed class ChatbotOptions
{
    public const string SectionName = "Chatbot";

    /// <summary>
    /// OpenAI API key. Prefer setting via environment variable / user-secrets.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model name, e.g. gpt-4o-mini.
    /// </summary>
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// System prompt template. Can include placeholders like {ShopName}.
    /// </summary>
    public string SystemPrompt { get; set; } = "You are a helpful shopping assistant.";

    public int MaxOutputTokens { get; set; } = 1500;
}

