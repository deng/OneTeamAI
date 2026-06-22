partial class Program
{
    static void MapAuthEndpoints(WebApplication app)
    {
        app.MapPost("/api/auth/register", async (
            HttpContext httpContext,
            RegisterRequest request,
            AppDbContext dbContext,
            AuditLogService auditLogService,
            IOptions<ChatbotOptions> chatbotOptions,
            IPasswordHasher<User> passwordHasher,
            CancellationToken cancellationToken) =>
        {
            var email = request.Email.Trim();
            var displayName = request.DisplayName.Trim();
            var password = request.Password.Trim();

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(password))
            {
                return BadRequestError("email, displayName and password are required", "register_required_fields");
            }

            if (!IsValidEmail(email))
            {
                return BadRequestError("email format is invalid", "invalid_email");
            }

            if (!IsStrongPassword(password))
            {
                return BadRequestError("password must be at least 8 characters and include upper, lower, and number", "weak_password");
            }

            var emailExists = await dbContext.Users
                .AnyAsync(user => user.Email == email, cancellationToken);

            if (emailExists)
            {
                return ConflictError("email already exists", "email_exists");
            }

            var user = new User
            {
                Email = email,
                DisplayName = displayName,
                CompanyName = string.IsNullOrWhiteSpace(request.CompanyName) ? null : request.CompanyName.Trim(),
            };
            user.PasswordHash = passwordHasher.HashPassword(user, password);

            var (session, accessToken) = CreateUserSession(httpContext, user, chatbotOptions.Value.SessionLifetimeDays);

            dbContext.Users.Add(user);
            dbContext.UserSessions.Add(session);
            await dbContext.SaveChangesAsync(cancellationToken);
            await auditLogService.WriteAsync(
                dbContext,
                httpContext,
                "auth.register",
                "user",
                user.Id,
                $"User {user.Email} registered.",
                user.Id,
                null,
                "success",
                cancellationToken);

            return Results.Created(
                $"/api/users/{user.Id}",
                new AuthResponse(accessToken, session.ExpiresAt, ToUserResponse(user)));
        })
        .Produces<AuthResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .WithName("Register")
        .WithTags("Auth");

        app.MapPost("/api/auth/login", async (
            HttpContext httpContext,
            LoginRequest request,
            AppDbContext dbContext,
            AuditLogService auditLogService,
            IOptions<ChatbotOptions> chatbotOptions,
            IPasswordHasher<User> passwordHasher,
            CancellationToken cancellationToken) =>
        {
            var email = request.Email.Trim();
            var password = request.Password.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequestError("email and password are required", "login_required_fields");
            }

            if (!IsValidEmail(email))
            {
                return BadRequestError("email format is invalid", "invalid_email");
            }

            var user = await dbContext.Users
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

            if (user is null)
            {
                return UnauthorizedError("invalid email or password", "invalid_credentials");
            }

            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return UnauthorizedError("invalid email or password", "invalid_credentials");
            }

            var (session, accessToken) = CreateUserSession(httpContext, user, chatbotOptions.Value.SessionLifetimeDays);
            dbContext.UserSessions.Add(session);
            await dbContext.SaveChangesAsync(cancellationToken);
            await auditLogService.WriteAsync(
                dbContext,
                httpContext,
                "auth.login",
                "user_session",
                session.Id,
                $"User {user.Email} logged in.",
                user.Id,
                null,
                "success",
                cancellationToken);

            return Results.Ok(new AuthResponse(accessToken, session.ExpiresAt, ToUserResponse(user)));
        })
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .WithName("Login")
        .WithTags("Auth");

        app.MapGet("/api/auth/sessions", async (
            HttpContext httpContext,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var currentSession = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            if (currentSession is null)
            {
                return UnauthorizedError();
            }

            var sessions = await dbContext.UserSessions
                .Where(x => x.UserId == currentSession.UserId)
                .OrderByDescending(x => x.CreatedAtMs)
                .ToListAsync(cancellationToken);

            return Results.Ok(sessions.Select(session => ToUserSessionResponse(session, currentSession.Id)).ToList());
        })
        .Produces<List<UserSessionResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .WithName("ListCurrentUserSessions")
        .WithTags("Auth");

        app.MapGet("/api/auth/me", async (
            HttpContext httpContext,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            if (session?.User is null)
            {
                return UnauthorizedError();
            }

            return Results.Ok(ToUserResponse(session.User));
        })
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .WithName("GetCurrentUser")
        .WithTags("Auth");

        app.MapPost("/api/auth/logout-all", async (
            HttpContext httpContext,
            AppDbContext dbContext,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            var currentSession = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            if (currentSession is null)
            {
                return UnauthorizedError();
            }

            var sessions = await dbContext.UserSessions
                .Where(x => x.UserId == currentSession.UserId && x.RevokedAt == null)
                .ToListAsync(cancellationToken);
            sessions = sessions
                .Where(x => x.ExpiresAt > DateTimeOffset.UtcNow)
                .ToList();

            var now = DateTimeOffset.UtcNow;
            foreach (var session in sessions)
            {
                session.RevokedAt = now;
                session.RevokedReason = session.Id == currentSession.Id ? "logout_all_current" : "logout_all";
                session.UpdatedAt = now;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await auditLogService.WriteAsync(
                dbContext,
                httpContext,
                "auth.logout_all",
                "user_session",
                currentSession.Id,
                $"User {currentSession.UserId} revoked {sessions.Count} session(s).",
                currentSession.UserId,
                null,
                "success",
                cancellationToken);

            return Results.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .WithName("LogoutAllSessions")
        .WithTags("Auth");

        app.MapPost("/api/auth/sessions/{sessionId:guid}/revoke", async (
            HttpContext httpContext,
            Guid sessionId,
            AppDbContext dbContext,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            var currentSession = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            if (currentSession is null)
            {
                return UnauthorizedError();
            }

            var targetSession = await dbContext.UserSessions
                .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == currentSession.UserId, cancellationToken);

            if (targetSession is null)
            {
                return NotFoundError("session not found", "session_not_found");
            }

            if (targetSession.RevokedAt is not null)
            {
                return ConflictError("session already revoked", "session_already_revoked");
            }

            targetSession.RevokedAt = DateTimeOffset.UtcNow;
            targetSession.RevokedReason = targetSession.Id == currentSession.Id ? "self_revoke" : "user_revoked";
            targetSession.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            await auditLogService.WriteAsync(
                dbContext,
                httpContext,
                "auth.revoke_session",
                "user_session",
                targetSession.Id,
                $"User {currentSession.UserId} revoked session {targetSession.Id}.",
                currentSession.UserId,
                null,
                "success",
                cancellationToken);

            return Results.Ok(ToUserSessionResponse(targetSession, currentSession.Id));
        })
        .Produces<UserSessionResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .WithName("RevokeUserSession")
        .WithTags("Auth");

        app.MapPost("/api/auth/logout", async (
            HttpContext httpContext,
            AppDbContext dbContext,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            if (session is null)
            {
                return UnauthorizedError();
            }

            session.RevokedAt = DateTimeOffset.UtcNow;
            session.RevokedReason = "logout";
            session.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            await auditLogService.WriteAsync(
                dbContext,
                httpContext,
                "auth.logout",
                "user_session",
                session.Id,
                $"User {session.UserId} logged out.",
                session.UserId,
                null,
                "success",
                cancellationToken);

            return Results.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .WithName("Logout")
        .WithTags("Auth");
    }
}
