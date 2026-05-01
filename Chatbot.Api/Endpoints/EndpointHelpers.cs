partial class Program
{
    static async Task WriteSseEventAsync(
        HttpContext httpContext,
        string eventName,
        object payload,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload);
        await httpContext.Response.WriteAsync($"event: {eventName}\n", cancellationToken);
        await httpContext.Response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await httpContext.Response.Body.FlushAsync(cancellationToken);
    }

    static (UserSession Session, string AccessToken) CreateUserSession(
        HttpContext httpContext,
        User user,
        int sessionLifetimeDays)
    {
        var accessToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var now = DateTimeOffset.UtcNow;
        var session = new UserSession
        {
            UserId = user.Id,
            TokenHash = ComputeTokenHash(accessToken),
            UserAgent = TrimToNullable(httpContext.Request.Headers.UserAgent.ToString(), 256),
            IpAddress = TrimToNullable(httpContext.Connection.RemoteIpAddress?.ToString(), 64),
            LastSeenAt = now,
            ExpiresAt = now.AddDays(Math.Clamp(sessionLifetimeDays, 1, 90)),
        };

        return (session, accessToken);
    }

    static async Task<UserSession?> GetCurrentSessionAsync(
        HttpContext httpContext,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await CleanupExpiredSessionsAsync(dbContext, cancellationToken);

        var token = ExtractBearerToken(httpContext.Request);
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var tokenHash = ComputeTokenHash(token);

        var session = await dbContext.UserSessions
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash
                    && x.RevokedAt == null,
                cancellationToken);

        if (session is null || session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        session.LastSeenAt = DateTimeOffset.UtcNow;
        session.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return session;
    }

    static async Task<IResult?> EnsureTeamAccessAsync(
        HttpContext httpContext,
        AppDbContext dbContext,
        Guid teamId,
        CancellationToken cancellationToken)
    {
        var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
        var user = session?.User;
        if (user is null)
        {
            return UnauthorizedError();
        }

        var hasAccess = await dbContext.Teams
            .Where(team => team.Id == teamId)
            .AnyAsync(
                team => team.OwnerUserId == user.Id
                    || team.Members.Any(member => member.UserId == user.Id && member.Status == MemberStatus.Active),
                cancellationToken);

        if (!hasAccess)
        {
            var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
            if (!teamExists)
            {
                return null;
            }

            return ForbiddenError();
        }

        return null;
    }

    static async Task<IResult?> EnsureTeamManagementAccessAsync(
        HttpContext httpContext,
        AppDbContext dbContext,
        Guid teamId,
        CancellationToken cancellationToken)
    {
        var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
        var user = session?.User;
        if (user is null)
        {
            return UnauthorizedError();
        }

        var hasManagementAccess = await dbContext.Teams
            .Where(team => team.Id == teamId)
            .AnyAsync(
                team => team.OwnerUserId == user.Id
                    || team.Members.Any(member =>
                        member.UserId == user.Id
                        && member.Status == MemberStatus.Active
                        && (member.Role == MemberRole.Owner || member.Role == MemberRole.Admin)),
                cancellationToken);

        if (!hasManagementAccess)
        {
            var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
            if (!teamExists)
            {
                return null;
            }

            return ForbiddenError();
        }

        return null;
    }

    static string? ExtractBearerToken(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return null;
        }

        var value = authorizationHeader.ToString();
        const string prefix = "Bearer ";
        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? value[prefix.Length..].Trim()
            : null;
    }

    static string ComputeTokenHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    static async Task<int> CleanupExpiredSessionsAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var expiredSessions = await dbContext.UserSessions
            .Where(x => x.RevokedAt == null)
            .ToListAsync(cancellationToken);
        expiredSessions = expiredSessions
            .Where(x => x.ExpiresAt <= now)
            .ToList();

        if (expiredSessions.Count == 0)
        {
            return 0;
        }

        foreach (var session in expiredSessions)
        {
            session.RevokedAt = now;
            session.RevokedReason = "expired";
            session.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return expiredSessions.Count;
    }

    static async Task<int> ExpirePendingInvitationsAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var invitations = await dbContext.TeamInvitations
            .Where(x => x.Status == InvitationStatus.Pending)
            .ToListAsync(cancellationToken);
        invitations = invitations
            .Where(x => x.ExpiresAt <= now)
            .ToList();

        if (invitations.Count == 0)
        {
            return 0;
        }

        foreach (var invitation in invitations)
        {
            invitation.Status = InvitationStatus.Expired;
            invitation.RespondedAt = now;
            invitation.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return invitations.Count;
    }

    static string? TrimToNullable(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        return trimmed.Length <= maxLength
            ? trimmed
            : trimmed[..maxLength];
    }

    static string? GetLatestUserText(IEnumerable<AiSdkMessage> messages)
    {
        var latestUserMessage = messages.LastOrDefault(message =>
            string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase));

        if (latestUserMessage is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(latestUserMessage.Content))
        {
            return latestUserMessage.Content;
        }

        var text = string.Concat(
            latestUserMessage.Parts
                .Where(part => string.Equals(part.Type, "text", StringComparison.OrdinalIgnoreCase))
                .Select(part => part.Text));

        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    static string BuildTicketTitle(string message)
    {
        const int maxLength = 48;
        var normalized = string.Join(' ', message.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)).Trim();

        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        return $"{normalized[..maxLength].TrimEnd()}...";
    }

    static IResult BadRequestError(string error, string code = "bad_request") =>
        Results.BadRequest(new ApiErrorResponse(error, code));

    static IResult ConflictError(string error, string code = "conflict") =>
        Results.Conflict(new ApiErrorResponse(error, code));

    static IResult NotFoundError(string error, string code = "not_found") =>
        Results.NotFound(new ApiErrorResponse(error, code));

    static IResult UnauthorizedError(string error = "authentication required", string code = "unauthorized") =>
        Results.Json(new ApiErrorResponse(error, code), statusCode: StatusCodes.Status401Unauthorized);

    static IResult ForbiddenError(string error = "forbidden", string code = "forbidden") =>
        Results.Json(new ApiErrorResponse(error, code), statusCode: StatusCodes.Status403Forbidden);

    static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email)
        && email.Contains('@')
        && email.IndexOf('@') > 0
        && email.IndexOf('@') < email.Length - 1;

    static bool IsStrongPassword(string? password) =>
        !string.IsNullOrWhiteSpace(password)
        && password.Length >= 8
        && password.Any(char.IsUpper)
        && password.Any(char.IsLower)
        && password.Any(char.IsDigit);

    static string NormalizeAiMemberTemplateKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-");
        normalized = Regex.Replace(normalized, @"-+", "-").Trim('-');
        return normalized.Length <= 64 ? normalized : normalized[..64].Trim('-');
    }

    static async Task<string> GenerateAiMemberTemplateKeyAsync(
        AppDbContext dbContext,
        string seed,
        CancellationToken cancellationToken)
    {
        var baseKey = NormalizeAiMemberTemplateKey(seed);
        if (string.IsNullOrWhiteSpace(baseKey))
        {
            baseKey = $"ai-template-{Guid.NewGuid():N}"[..24];
        }

        var candidate = baseKey;
        var suffix = 2;
        while (await dbContext.AiMemberTemplates.AnyAsync(template => template.Key == candidate, cancellationToken))
        {
            var suffixText = $"-{suffix}";
            var prefixLength = Math.Min(baseKey.Length, 64 - suffixText.Length);
            candidate = $"{baseKey[..prefixLength].Trim('-')}{suffixText}";
            suffix += 1;
        }

        return candidate;
    }

    static async Task<IntegrationConnection?> LoadIntegrationConnectionAsync(
        AppDbContext dbContext,
        Guid teamId,
        Guid connectionId,
        CancellationToken cancellationToken) =>
        await dbContext.IntegrationConnections
            .FirstOrDefaultAsync(
                connection => connection.Id == connectionId && connection.TeamId == teamId,
                cancellationToken);

    static TProvider? ResolveAdapter<TProvider>(
        IEnumerable<TProvider> providers,
        ExternalSystemType externalSystemType)
        where TProvider : IExternalSystemAdapter =>
        providers.FirstOrDefault(provider => provider.CanHandle(externalSystemType));
}
