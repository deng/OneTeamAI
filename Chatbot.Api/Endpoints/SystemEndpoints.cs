partial class Program
{
    static void MapSystemEndpoints(WebApplication app)
    {
        app.MapGet("/", (IOptions<ChatbotOptions> options) => Results.Ok(new ApiRootResponse(
            "chatbot-api",
            "Microsoft Agent Framework",
            options.Value.AgentName,
            new[]
            {
                "/api/auth/register",
                "/api/auth/login",
                "/api/auth/me",
                "/api/invitations/me",
                "/api/teams",
                "/api/teams/me",
                "/api/teams/{teamId}",
                "/api/teams/{teamId}/members",
                "/api/teams/{teamId}/members/ai",
                "/api/teams/{teamId}/members/{memberId}",
                "/api/teams/{teamId}/invitations",
                "/api/teams/{teamId}/projects",
                "/api/teams/{teamId}/concierge-apps",
                "/api/teams/{teamId}/customers",
                "/api/teams/{teamId}/tickets",
                "/api/teams/{teamId}/tickets/{ticketId}/workflows",
                "/api/teams/{teamId}/workflows",
                "/api/concierge-apps/{conciergeAppId}/conversations",
                "/swagger",
                "/swagger/v1/swagger.json",
                "/api/chat",
                "/api/chat/stream",
                "/agui"
            })))
        .Produces<ApiRootResponse>(StatusCodes.Status200OK)
        .WithName("GetApiRoot")
        .WithTags("System");

        app.MapGet("/health", async (
            AppDbContext dbContext,
            IOptions<ChatbotOptions> chatbotOptions,
            IWebHostEnvironment environment,
            CancellationToken cancellationToken) =>
        {
            var databaseReachable = await dbContext.Database.CanConnectAsync(cancellationToken);
            if (databaseReachable)
            {
                await CleanupExpiredSessionsAsync(dbContext, cancellationToken);
                await ExpirePendingInvitationsAsync(dbContext, cancellationToken);
            }

            var now = DateTimeOffset.UtcNow;
            var userSessions = databaseReachable
                ? await dbContext.UserSessions.ToListAsync(cancellationToken)
                : [];
            var teamInvitations = databaseReachable
                ? await dbContext.TeamInvitations.ToListAsync(cancellationToken)
                : [];

            var activeSessionCount = databaseReachable
                ? userSessions.Count(session => session.RevokedAt == null && session.ExpiresAt > now)
                : 0;
            var expiredSessionCount = databaseReachable
                ? userSessions.Count(session => session.ExpiresAt <= now)
                : 0;
            var teamCount = databaseReachable
                ? await dbContext.Teams.CountAsync(cancellationToken)
                : 0;
            var pendingInvitationCount = databaseReachable
                ? teamInvitations.Count(invitation => invitation.Status == InvitationStatus.Pending && invitation.ExpiresAt > now)
                : 0;
            var expiredInvitationCount = databaseReachable
                ? teamInvitations.Count(invitation => invitation.Status == InvitationStatus.Expired)
                : 0;
            var auditLogCount = databaseReachable
                ? await dbContext.AuditLogs.CountAsync(cancellationToken)
                : 0;

            var chatbotConfigured =
                !string.IsNullOrWhiteSpace(chatbotOptions.Value.ApiKey)
                && !string.IsNullOrWhiteSpace(chatbotOptions.Value.Model);

            return Results.Ok(new HealthResponse(
                databaseReachable && chatbotConfigured ? "ok" : "degraded",
                environment.EnvironmentName,
                databaseReachable,
                chatbotConfigured,
                activeSessionCount,
                expiredSessionCount,
                teamCount,
                pendingInvitationCount,
                expiredInvitationCount,
                auditLogCount,
                DateTimeOffset.UtcNow));
        })
            .Produces<HealthResponse>(StatusCodes.Status200OK)
            .WithName("GetHealth")
            .WithTags("System");

        app.MapGet("/api/audit-logs/me", async (
            HttpContext httpContext,
            int? take,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            var currentUser = session?.User;
            if (currentUser is null)
            {
                return UnauthorizedError();
            }

            var limit = Math.Clamp(take ?? 20, 1, 100);
            var logs = await dbContext.AuditLogs
                .Where(log => log.UserId == currentUser.Id)
                .Include(log => log.User)
                .OrderByDescending(log => log.CreatedAtMs)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return Results.Ok(logs.Select(ToAuditLogResponse).ToList());
        })
        .Produces<List<AuditLogResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .WithName("ListMyAuditLogs")
        .WithTags("Audit");
    }
}
