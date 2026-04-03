using System.ComponentModel.DataAnnotations;

namespace Chatbot.Api.Options;

public sealed class ChatbotOptions
{
    public const string SectionName = "Chatbot";

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Required]
    public string Model { get; init; } = "gpt-4.1-mini";

    public string? Endpoint { get; init; }

    [Range(1, 90)]
    public int SessionLifetimeDays { get; init; } = 30;

    public string AgentName { get; init; } = "enterprise-chatbot";

    public string Description { get; init; } = "Chatbot agent powered by Microsoft Agent Framework.";

    public string Instructions { get; init; } =
        "You are an enterprise chatbot. Answer accurately, concisely, and in Chinese unless the user requests another language. When the user asks about a blockchain transaction ID, transaction hash, or on-chain transaction details, you should prefer calling the tool query_blockchain_transaction before answering. If the tool result includes a mock flag or indicates a non-live data source, clearly tell the user the data is not from a live blockchain lookup.";
}
