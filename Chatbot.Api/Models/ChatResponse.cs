namespace Chatbot.Api.Models;

public sealed record ChatResponse(
    string SessionId,
    string Message);
