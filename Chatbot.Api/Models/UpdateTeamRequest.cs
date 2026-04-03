namespace Chatbot.Api.Models;

public sealed record UpdateTeamRequest(
    string Name,
    string? Description = null,
    string? BrandName = null);
