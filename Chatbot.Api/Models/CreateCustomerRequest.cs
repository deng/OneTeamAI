using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record CreateCustomerRequest(
    string DisplayName,
    string? Email = null,
    string? PhoneNumber = null,
    string? CompanyName = null,
    string? SourceLabel = null,
    string? Tags = null,
    CustomerFollowUpStatus FollowUpStatus = CustomerFollowUpStatus.New,
    DateTimeOffset? LastContactedAt = null,
    Guid? ProjectId = null,
    string? Notes = null);
