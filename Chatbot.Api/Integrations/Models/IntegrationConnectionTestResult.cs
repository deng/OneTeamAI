namespace Chatbot.Api.Integrations.Models;

public sealed record IntegrationConnectionTestResult(
    bool IsReachable,
    bool IsAuthenticated,
    string Message,
    DateTimeOffset CheckedAt,
    string? SystemVersion = null);
