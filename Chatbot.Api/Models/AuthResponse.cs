namespace Chatbot.Api.Models;

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    UserResponse User);
