namespace Chatbot.Api.Models;

public sealed record ApiRootResponse(
    string Service,
    string Framework,
    string Agent,
    IReadOnlyList<string> Endpoints);
