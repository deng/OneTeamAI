using Chatbot.Api.Domain.Entities;
using Chatbot.Api.Infrastructure.Persistence;

namespace Chatbot.Api.Services;

public sealed class AuditLogService
{
    public async Task WriteAsync(
        AppDbContext dbContext,
        HttpContext? httpContext,
        string actionType,
        string entityType,
        Guid? entityId,
        string summary,
        Guid? userId = null,
        Guid? teamId = null,
        string result = "success",
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            TeamId = teamId,
            UserId = userId,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId,
            Summary = summary,
            Result = result,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
        };

        dbContext.AuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
