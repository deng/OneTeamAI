namespace Chatbot.Api.Models;

public sealed record HealthResponse(
    string Status,
    string Environment,
    bool DatabaseReachable,
    bool ChatbotConfigured,
    int ActiveSessionCount,
    int ExpiredSessionCount,
    int TeamCount,
    int PendingInvitationCount,
    int ExpiredInvitationCount,
    int AuditLogCount,
    DateTimeOffset CheckedAt);
