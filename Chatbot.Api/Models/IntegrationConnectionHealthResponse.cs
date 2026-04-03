namespace Chatbot.Api.Models;

public sealed record IntegrationConnectionHealthResponse(
    bool IsReachable,
    bool IsAuthenticated,
    string Message,
    DateTimeOffset CheckedAt,
    string? SystemVersion = null);
