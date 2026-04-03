namespace Chatbot.Api.Models;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName,
    string? CompanyName = null);
