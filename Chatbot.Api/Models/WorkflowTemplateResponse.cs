namespace Chatbot.Api.Models;

public sealed record WorkflowTemplateResponse(
    string Key,
    string Scope,
    string Label,
    string Goal,
    string Summary);
