namespace Chatbot.Api.Models;

public sealed record ChatRequest(
    string Message,
    string? SessionId = null);
