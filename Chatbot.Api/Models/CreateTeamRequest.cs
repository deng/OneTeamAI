namespace Chatbot.Api.Models;

public sealed record CreateTeamRequest(
    string Name,
    string? Description = null,
    string? BrandName = null);
