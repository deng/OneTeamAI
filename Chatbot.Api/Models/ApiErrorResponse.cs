namespace Chatbot.Api.Models;

public sealed record ApiErrorResponse(string Error, string Code = "api_error");
