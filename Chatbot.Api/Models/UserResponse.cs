namespace Chatbot.Api.Models;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string? CompanyName);
