namespace Chatbot.Api.Models;

public sealed class AiSdkChatRequest
{
    public string? Id { get; init; }

    public List<AiSdkMessage> Messages { get; init; } = [];
}

public sealed class AiSdkMessage
{
    public string? Role { get; init; }

    public string? Content { get; init; }

    public List<AiSdkMessagePart> Parts { get; init; } = [];
}

public sealed class AiSdkMessagePart
{
    public string? Type { get; init; }

    public string? Text { get; init; }
}
