using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Chatbot.Api.Domain.Entities;
using Chatbot.Api.Domain.Enums;
using Chatbot.Api.Integrations.DependencyInjection;
using Chatbot.Api.Integrations.Models;
using Chatbot.Api.Integrations.Providers;
using Chatbot.Api.Infrastructure.Persistence;
using Chatbot.Api.Models;
using Chatbot.Api.Options;
using Chatbot.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173", "http://127.0.0.1:5173"];

builder.Services
    .AddOptions<ChatbotOptions>()
    .Bind(builder.Configuration.GetSection(ChatbotOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ApiKey),
        "Chatbot:ApiKey is required.")
    .ValidateOnStart();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=chatbot.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Chatbot API",
        Version = "v1",
        Description = "AI virtual team workspace API with chatbot, team, project, concierge app, conversation, and ticket endpoints.",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "Bearer",
        In = ParameterLocation.Header,
        Description = "Bearer access token.",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAGUI();
builder.Services.AddSingleton<ChatbotAgentRuntime>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<AgentWorkflowOrchestrator>();
builder.Services.AddScoped<AgentWorkflowExecutionService>();
builder.Services.AddScoped<AgentWorkflowWritebackService>();
builder.Services.AddExternalSystemAdapters();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseCors("frontend");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Chatbot API v1");
    options.RoutePrefix = "swagger";
});

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

    var activeSessionCount = databaseReachable
        ? await dbContext.UserSessions.CountAsync(
            session => session.RevokedAt == null && session.ExpiresAt > DateTimeOffset.UtcNow,
            cancellationToken)
        : 0;
    var expiredSessionCount = databaseReachable
        ? await dbContext.UserSessions.CountAsync(
            session => session.ExpiresAt <= DateTimeOffset.UtcNow,
            cancellationToken)
        : 0;
    var teamCount = databaseReachable
        ? await dbContext.Teams.CountAsync(cancellationToken)
        : 0;
    var pendingInvitationCount = databaseReachable
        ? await dbContext.TeamInvitations.CountAsync(
            invitation => invitation.Status == InvitationStatus.Pending && invitation.ExpiresAt > DateTimeOffset.UtcNow,
            cancellationToken)
        : 0;
    var expiredInvitationCount = databaseReachable
        ? await dbContext.TeamInvitations.CountAsync(
            invitation => invitation.Status == InvitationStatus.Expired,
            cancellationToken)
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
        .OrderByDescending(log => log.CreatedAt)
        .Take(limit)
        .ToListAsync(cancellationToken);

    return Results.Ok(logs.Select(ToAuditLogResponse).ToList());
})
.Produces<List<AuditLogResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.WithName("ListMyAuditLogs")
.WithTags("Audit");

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
        .OrderByDescending(x => x.CreatedAt)
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
        .Where(x => x.UserId == currentSession.UserId && x.RevokedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow)
        .ToListAsync(cancellationToken);

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

app.MapGet("/api/ai-member-templates", (HttpContext httpContext) =>
{
    if (string.IsNullOrWhiteSpace(ExtractBearerToken(httpContext.Request)))
    {
        return UnauthorizedError();
    }

    return Results.Ok(GetDefaultAiMemberTemplates());
})
.Produces<List<AiMemberTemplateResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.WithName("ListAiMemberTemplates")
.WithTags("Members");

app.MapGet("/api/workflow-templates", (HttpContext httpContext, string? scope) =>
{
    if (string.IsNullOrWhiteSpace(ExtractBearerToken(httpContext.Request)))
    {
        return UnauthorizedError();
    }

    var templates = GetDefaultWorkflowTemplates();
    if (!string.IsNullOrWhiteSpace(scope))
    {
        templates = templates
            .Where(template => string.Equals(template.Scope, scope.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    return Results.Ok(templates);
})
.Produces<List<WorkflowTemplateResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.WithName("ListWorkflowTemplates")
.WithTags("Workflows");

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

app.MapPost("/api/teams", async (
    HttpContext httpContext,
    CreateTeamRequest request,
    AppDbContext dbContext,
    AuditLogService auditLogService,
    CancellationToken cancellationToken) =>
{
    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    var owner = session?.User;
    if (owner is null)
    {
        return UnauthorizedError();
    }

    var teamName = request.Name.Trim();
    if (string.IsNullOrWhiteSpace(teamName))
    {
        return BadRequestError("name is required", "team_name_required");
    }

    var team = new Team
    {
        Name = teamName,
        Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
        BrandName = string.IsNullOrWhiteSpace(request.BrandName) ? null : request.BrandName.Trim(),
        OwnerUserId = owner.Id,
    };

    var ownerMember = new Member
    {
        Team = team,
        UserId = owner.Id,
        MemberType = MemberType.Human,
        Role = MemberRole.Owner,
        Status = MemberStatus.Active,
        DisplayName = owner.DisplayName,
        Title = "Owner",
    };

    dbContext.Teams.Add(team);
    dbContext.Members.Add(ownerMember);
    await dbContext.SaveChangesAsync(cancellationToken);
    await auditLogService.WriteAsync(
        dbContext,
        httpContext,
        "team.create",
        "team",
        team.Id,
        $"Team {team.Name} created.",
        owner.Id,
        team.Id,
        "success",
        cancellationToken);

    return Results.Created(
        $"/api/teams/{team.Id}",
        new TeamResponse(
            team.Id,
            team.Name,
            team.Description,
            team.BrandName,
            team.OwnerUserId,
            ownerMember.Id));
})
.Produces<TeamResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.WithName("CreateTeam")
.WithTags("Teams");

app.MapGet("/api/teams/me", async (
    HttpContext httpContext,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    var currentUser = session?.User;
    if (currentUser is null)
    {
        return UnauthorizedError();
    }

    var teams = await dbContext.Teams
        .Where(team =>
            team.OwnerUserId == currentUser.Id
            || team.Members.Any(member => member.UserId == currentUser.Id && member.Status == MemberStatus.Active))
        .Include(team => team.Members)
        .OrderBy(team => team.Name)
        .ToListAsync(cancellationToken);

    var result = teams.Select(team =>
    {
        var currentMember = team.Members
            .FirstOrDefault(member => member.UserId == currentUser.Id && member.Status == MemberStatus.Active);

        return new TeamSummaryResponse(
            team.Id,
            team.Name,
            team.Description,
            team.BrandName,
            team.OwnerUserId,
            currentMember?.Id,
            currentMember?.Role);
    }).ToList();

    return Results.Ok(result);
})
.Produces<List<TeamSummaryResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.WithName("ListMyTeams")
.WithTags("Teams");

app.MapGet("/api/teams/{teamId:guid}", async (
    HttpContext httpContext,
    Guid teamId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var team = await dbContext.Teams
        .Include(x => x.OwnerUser)
        .Include(x => x.Members)
        .Include(x => x.Projects)
        .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);

    if (team is null)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    return Results.Ok(
        new TeamDetailResponse(
            team.Id,
            team.Name,
            team.Description,
            team.BrandName,
            team.OwnerUser is null
                ? null
                : new TeamOwnerResponse(
                    team.OwnerUser.Id,
                    team.OwnerUser.DisplayName,
                    team.OwnerUser.Email),
            team.Members.Count,
            team.Projects.Count));
})
.Produces<TeamDetailResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("GetTeam")
.WithTags("Teams");

app.MapGet("/api/teams/{teamId:guid}/audit-logs", async (
    HttpContext httpContext,
    Guid teamId,
    int? take,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
    if (!teamExists)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var limit = Math.Clamp(take ?? 50, 1, 200);
    var logs = await dbContext.AuditLogs
        .Where(log => log.TeamId == teamId)
        .Include(log => log.User)
        .OrderByDescending(log => log.CreatedAt)
        .Take(limit)
        .ToListAsync(cancellationToken);

    return Results.Ok(logs.Select(ToAuditLogResponse).ToList());
})
.Produces<List<AuditLogResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("ListTeamAuditLogs")
.WithTags("Audit");

app.MapPatch("/api/teams/{teamId:guid}", async (
    HttpContext httpContext,
    Guid teamId,
    UpdateTeamRequest request,
    AppDbContext dbContext,
    AuditLogService auditLogService,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var team = await dbContext.Teams
        .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);

    if (team is null)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var teamName = request.Name.Trim();
    if (string.IsNullOrWhiteSpace(teamName))
    {
        return Results.BadRequest(new ApiErrorResponse("name is required"));
    }

    team.Name = teamName;
    team.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
    team.BrandName = string.IsNullOrWhiteSpace(request.BrandName) ? null : request.BrandName.Trim();
    team.UpdatedAt = DateTimeOffset.UtcNow;

    await dbContext.SaveChangesAsync(cancellationToken);
    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    await auditLogService.WriteAsync(
        dbContext,
        httpContext,
        "team.update",
        "team",
        team.Id,
        $"Team {team.Name} updated.",
        session?.UserId,
        team.Id,
        "success",
        cancellationToken);

    return Results.Ok(new TeamResponse(
        team.Id,
        team.Name,
        team.Description,
        team.BrandName,
        team.OwnerUserId,
        Guid.Empty));
})
.Produces<TeamResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("UpdateTeam")
.WithTags("Teams");

app.MapGet("/api/teams/{teamId:guid}/members", async (
    HttpContext httpContext,
    Guid teamId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
    if (!teamExists)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var memberEntities = await dbContext.Members
        .Where(member => member.TeamId == teamId)
        .Include(member => member.AiProfile)
        .OrderBy(member => member.MemberType)
        .ThenBy(member => member.DisplayName)
        .ToListAsync(cancellationToken);

    var members = memberEntities.Select(member => ToMemberResponse(member)).ToList();

    return Results.Ok(members);
})
.Produces<List<MemberResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("ListTeamMembers")
.WithTags("Members");

app.MapPost("/api/teams/{teamId:guid}/members/ai", async (
    HttpContext httpContext,
    Guid teamId,
    CreateAiMemberRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var team = await dbContext.Teams
        .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);

    if (team is null)
    {
        return Results.NotFound(new ApiErrorResponse("team was not found"));
    }

    var displayName = request.DisplayName.Trim();
    var jobTitle = request.JobTitle.Trim();
    var responsibilitySummary = request.ResponsibilitySummary.Trim();

    if (string.IsNullOrWhiteSpace(displayName) ||
        string.IsNullOrWhiteSpace(jobTitle) ||
        string.IsNullOrWhiteSpace(responsibilitySummary))
    {
        return BadRequestError("displayName, jobTitle and responsibilitySummary are required", "ai_member_required_fields");
    }

    var member = new Member
    {
        TeamId = team.Id,
        MemberType = MemberType.Ai,
        Role = MemberRole.AiEmployee,
        Status = MemberStatus.Active,
        DisplayName = displayName,
        Title = string.IsNullOrWhiteSpace(request.Title) ? jobTitle : request.Title.Trim(),
        AiProfile = new AIMemberProfile
        {
            TemplateKey = string.IsNullOrWhiteSpace(request.TemplateKey) ? null : request.TemplateKey.Trim(),
            JobTitle = jobTitle,
            ResponsibilitySummary = responsibilitySummary,
            PermissionBoundary = string.IsNullOrWhiteSpace(request.PermissionBoundary) ? null : request.PermissionBoundary.Trim(),
            SystemPrompt = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : request.SystemPrompt.Trim(),
            AllowedTools = string.IsNullOrWhiteSpace(request.AllowedTools) ? null : request.AllowedTools.Trim(),
            ExecutableActions = string.IsNullOrWhiteSpace(request.ExecutableActions) ? null : request.ExecutableActions.Trim(),
            KnowledgeScope = string.IsNullOrWhiteSpace(request.KnowledgeScope) ? null : request.KnowledgeScope.Trim(),
            IsAutonomous = request.IsAutonomous,
        }
    };

    dbContext.Members.Add(member);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/teams/{team.Id}/members/{member.Id}", ToMemberResponse(member));
})
.Produces<MemberResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("CreateAiMember")
.WithTags("Members");

app.MapPost("/api/teams/{teamId:guid}/members/human", async (
    HttpContext httpContext,
    Guid teamId,
    CreateHumanMemberRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
    if (!teamExists)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var email = request.Email.Trim();
    if (string.IsNullOrWhiteSpace(email))
    {
        return BadRequestError("email is required", "email_required");
    }

    if (!IsValidEmail(email))
    {
        return BadRequestError("email format is invalid", "invalid_email");
    }

    if (request.Role is MemberRole.AiEmployee or MemberRole.Owner)
    {
        return BadRequestError("role must be admin, operator, or viewer for human members", "invalid_member_role");
    }

    var user = await dbContext.Users
        .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    if (user is null)
    {
        return Results.NotFound(new ApiErrorResponse("user was not found"));
    }

    var existingMembership = await dbContext.Members
        .FirstOrDefaultAsync(
            x => x.TeamId == teamId && x.UserId == user.Id,
            cancellationToken);

    if (existingMembership is not null)
    {
        return Results.Conflict(new ApiErrorResponse("user is already a member of the team"));
    }

    var member = new Member
    {
        TeamId = teamId,
        UserId = user.Id,
        MemberType = MemberType.Human,
        Role = request.Role,
        Status = MemberStatus.Active,
        DisplayName = user.DisplayName,
        Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
    };

    dbContext.Members.Add(member);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/teams/{teamId}/members/{member.Id}", ToMemberResponse(member));
})
.Produces<MemberResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
.WithName("CreateHumanMember")
.WithTags("Members");

app.MapPatch("/api/teams/{teamId:guid}/members/{memberId:guid}", async (
    HttpContext httpContext,
    Guid teamId,
    Guid memberId,
    UpdateMemberRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    var currentUser = session?.User;
    if (currentUser is null)
    {
        return UnauthorizedError();
    }

    var member = await dbContext.Members
        .Include(x => x.AiProfile)
        .FirstOrDefaultAsync(x => x.Id == memberId && x.TeamId == teamId, cancellationToken);

    if (member is null)
    {
        return Results.NotFound(new ApiErrorResponse("member was not found"));
    }

    if (member.Role == MemberRole.Owner)
    {
        return Results.BadRequest(new ApiErrorResponse("owner member cannot be updated"));
    }

    if (member.UserId == currentUser.Id)
    {
        return Results.BadRequest(new ApiErrorResponse("you cannot change your own role here"));
    }

    if (member.MemberType == MemberType.Ai)
    {
        if (request.Role != MemberRole.AiEmployee)
        {
            return Results.BadRequest(new ApiErrorResponse("ai members must keep ai employee role"));
        }
    }
    else if (request.Role is MemberRole.Owner or MemberRole.AiEmployee)
    {
        return Results.BadRequest(new ApiErrorResponse("human members can only be admin, operator, or viewer"));
    }

    member.Role = request.Role;
    member.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();
    member.UpdatedAt = DateTimeOffset.UtcNow;

    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Ok(ToMemberResponse(member));
})
.Produces<MemberResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("UpdateMember")
.WithTags("Members");

app.MapDelete("/api/teams/{teamId:guid}/members/{memberId:guid}", async (
    HttpContext httpContext,
    Guid teamId,
    Guid memberId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    var currentUser = session?.User;
    if (currentUser is null)
    {
        return UnauthorizedError();
    }

    var member = await dbContext.Members
        .FirstOrDefaultAsync(x => x.Id == memberId && x.TeamId == teamId, cancellationToken);

    if (member is null)
    {
        return Results.NotFound(new ApiErrorResponse("member was not found"));
    }

    if (member.Role == MemberRole.Owner)
    {
        return Results.BadRequest(new ApiErrorResponse("owner member cannot be removed"));
    }

    if (member.UserId == currentUser.Id)
    {
        return Results.BadRequest(new ApiErrorResponse("you cannot remove yourself from the team here"));
    }

    member.Status = MemberStatus.Archived;
    member.UpdatedAt = DateTimeOffset.UtcNow;

    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("RemoveMember")
.WithTags("Members");

app.MapPost("/api/teams/{teamId:guid}/invitations", async (
    HttpContext httpContext,
    Guid teamId,
    CreateInvitationRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    var currentUser = session?.User;
    if (currentUser is null)
    {
        return UnauthorizedError();
    }

    var team = await dbContext.Teams
        .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);
    if (team is null)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var email = request.Email.Trim();
    if (string.IsNullOrWhiteSpace(email))
    {
        return BadRequestError("email is required", "email_required");
    }

    if (!IsValidEmail(email))
    {
        return BadRequestError("email format is invalid", "invalid_email");
    }

    if (request.Role is MemberRole.Owner or MemberRole.AiEmployee)
    {
        return BadRequestError("role must be admin, operator, or viewer for invitations", "invalid_invitation_role");
    }

    var existingMember = await dbContext.Members
        .AnyAsync(x => x.TeamId == teamId && x.User != null && x.User.Email == email, cancellationToken);
    if (existingMember)
    {
        return Results.Conflict(new ApiErrorResponse("user is already a member of the team"));
    }

    var pendingInvitation = await dbContext.TeamInvitations
        .FirstOrDefaultAsync(
            x => x.TeamId == teamId
                && x.Email == email
                && x.Status == InvitationStatus.Pending
                && x.ExpiresAt > DateTimeOffset.UtcNow,
            cancellationToken);

    if (pendingInvitation is not null)
    {
        return ConflictError("a pending invitation already exists for this email", "pending_invitation_exists");
    }

    var invitation = new TeamInvitation
    {
        TeamId = teamId,
        Email = email,
        Role = request.Role,
        Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
        Status = InvitationStatus.Pending,
        InvitedByUserId = currentUser.Id,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(Math.Clamp(request.ExpiresInDays, 1, 30)),
    };

    dbContext.TeamInvitations.Add(invitation);
    await dbContext.SaveChangesAsync(cancellationToken);

    invitation.Team = team;
    invitation.InvitedByUser = currentUser;

    return Results.Created($"/api/teams/{teamId}/invitations/{invitation.Id}", ToInvitationResponse(invitation));
})
.Produces<InvitationResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
.WithName("CreateInvitation")
.WithTags("Invitations");

app.MapGet("/api/teams/{teamId:guid}/invitations", async (
    HttpContext httpContext,
    Guid teamId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var invitations = await dbContext.TeamInvitations
        .Where(x => x.TeamId == teamId)
        .Include(x => x.Team)
        .Include(x => x.InvitedByUser)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync(cancellationToken);

    return Results.Ok(invitations.Select(ToInvitationResponse).ToList());
})
.Produces<List<InvitationResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.WithName("ListTeamInvitations")
.WithTags("Invitations");

app.MapGet("/api/invitations/me", async (
    HttpContext httpContext,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    var currentUser = session?.User;
    if (currentUser is null)
    {
        return UnauthorizedError();
    }

    var invitations = await dbContext.TeamInvitations
        .Where(x => x.Email == currentUser.Email && x.Status == InvitationStatus.Pending)
        .Include(x => x.Team)
        .Include(x => x.InvitedByUser)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync(cancellationToken);

    await ExpirePendingInvitationsAsync(dbContext, cancellationToken);

    return Results.Ok(invitations
        .Where(x => x.ExpiresAt > DateTimeOffset.UtcNow && x.Status == InvitationStatus.Pending)
        .Select(ToInvitationResponse)
        .ToList());
})
.Produces<List<InvitationResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.WithName("ListMyInvitations")
.WithTags("Invitations");

app.MapPost("/api/invitations/{invitationId:guid}/accept", async (
    HttpContext httpContext,
    Guid invitationId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    var currentUser = session?.User;
    if (currentUser is null)
    {
        return UnauthorizedError();
    }

    var invitation = await dbContext.TeamInvitations
        .Include(x => x.Team)
        .Include(x => x.InvitedByUser)
        .FirstOrDefaultAsync(x => x.Id == invitationId, cancellationToken);

    if (invitation is null)
    {
        return Results.NotFound(new ApiErrorResponse("invitation was not found"));
    }

    if (!string.Equals(invitation.Email, currentUser.Email, StringComparison.OrdinalIgnoreCase))
    {
        return Results.Json(new ApiErrorResponse("forbidden"), statusCode: StatusCodes.Status403Forbidden);
    }

    if (invitation.Status != InvitationStatus.Pending)
    {
        return Results.BadRequest(new ApiErrorResponse("invitation is not pending"));
    }

    if (invitation.ExpiresAt <= DateTimeOffset.UtcNow)
    {
        invitation.Status = InvitationStatus.Expired;
        invitation.RespondedAt = DateTimeOffset.UtcNow;
        invitation.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.BadRequest(new ApiErrorResponse("invitation has expired"));
    }

    var existingMember = await dbContext.Members
        .FirstOrDefaultAsync(x => x.TeamId == invitation.TeamId && x.UserId == currentUser.Id, cancellationToken);

    if (existingMember is null)
    {
        dbContext.Members.Add(new Member
        {
            TeamId = invitation.TeamId,
            UserId = currentUser.Id,
            MemberType = MemberType.Human,
            Role = invitation.Role,
            Status = MemberStatus.Active,
            DisplayName = currentUser.DisplayName,
            Title = invitation.Title,
        });
    }

    invitation.Status = InvitationStatus.Accepted;
    invitation.AcceptedByUserId = currentUser.Id;
    invitation.RespondedAt = DateTimeOffset.UtcNow;
    invitation.UpdatedAt = DateTimeOffset.UtcNow;

    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Ok(ToInvitationResponse(invitation));
})
.Produces<InvitationResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("AcceptInvitation")
.WithTags("Invitations");

app.MapPost("/api/invitations/{invitationId:guid}/revoke", async (
    HttpContext httpContext,
    Guid invitationId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var invitation = await dbContext.TeamInvitations
        .Include(x => x.Team)
        .Include(x => x.InvitedByUser)
        .FirstOrDefaultAsync(x => x.Id == invitationId, cancellationToken);

    if (invitation is null)
    {
        return Results.NotFound(new ApiErrorResponse("invitation was not found"));
    }

    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, invitation.TeamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    if (invitation.Status != InvitationStatus.Pending)
    {
        return Results.BadRequest(new ApiErrorResponse("only pending invitations can be revoked"));
    }

    invitation.Status = InvitationStatus.Revoked;
    invitation.RespondedAt = DateTimeOffset.UtcNow;
    invitation.UpdatedAt = DateTimeOffset.UtcNow;
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Ok(ToInvitationResponse(invitation));
})
.Produces<InvitationResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("RevokeInvitation")
.WithTags("Invitations");

app.MapPost("/api/teams/{teamId:guid}/projects", async (
    HttpContext httpContext,
    Guid teamId,
    CreateProjectRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var team = await dbContext.Teams
        .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);

    if (team is null)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    Member? leadMember = null;
    if (request.LeadMemberId.HasValue)
    {
        leadMember = await dbContext.Members
            .FirstOrDefaultAsync(
                member => member.Id == request.LeadMemberId.Value && member.TeamId == teamId,
                cancellationToken);

        if (leadMember is null)
        {
            return Results.BadRequest(new ApiErrorResponse("leadMemberId does not belong to the team"));
        }
    }

    var projectName = request.Name.Trim();
    if (string.IsNullOrWhiteSpace(projectName))
    {
        return Results.BadRequest(new ApiErrorResponse("name is required"));
    }

    var participantMemberIds = request.LeadMemberId is null
        ? []
        : new List<Guid> { request.LeadMemberId.Value };

    var project = new Project
    {
        TeamId = teamId,
        Name = projectName,
        Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
        StageLabel = TrimToNullable(request.StageLabel, 128),
        Summary = TrimToNullable(request.Summary, 4096),
        RiskSummary = TrimToNullable(request.RiskSummary, 4096),
        NextSteps = TrimToNullable(request.NextSteps, 4096),
        Status = ProjectStatus.Draft,
        LeadMemberId = leadMember?.Id,
    };

    dbContext.Projects.Add(project);
    await dbContext.SaveChangesAsync(cancellationToken);

    if (participantMemberIds.Count > 0)
    {
        dbContext.ProjectMembers.AddRange(participantMemberIds.Select(memberId => new ProjectMember
        {
            ProjectId = project.Id,
            MemberId = memberId,
            RoleLabel = memberId == project.LeadMemberId ? "负责人" : "参与成员",
        }));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    return Results.Created(
        $"/api/teams/{teamId}/projects/{project.Id}",
        ToProjectResponse(project, 0, 0, participantMemberIds));
})
.Produces<ProjectResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("CreateProject")
.WithTags("Projects");

app.MapGet("/api/teams/{teamId:guid}/projects", async (
    HttpContext httpContext,
    Guid teamId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
    if (!teamExists)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var projects = await dbContext.Projects
        .Include(project => project.ProjectMembers)
        .Where(project => project.TeamId == teamId)
        .OrderBy(project => project.CreatedAt)
        .ToListAsync(cancellationToken);

    var projectIds = projects.Select(project => project.Id).ToList();

    var ticketCounts = await dbContext.Tickets
        .Where(ticket => projectIds.Contains(ticket.ProjectId))
        .GroupBy(ticket => ticket.ProjectId)
        .Select(group => new { ProjectId = group.Key, Count = group.Count() })
        .ToDictionaryAsync(item => item.ProjectId, item => item.Count, cancellationToken);

    var customerCounts = await dbContext.Customers
        .Where(customer => customer.ProjectId.HasValue && projectIds.Contains(customer.ProjectId.Value))
        .GroupBy(customer => customer.ProjectId!.Value)
        .Select(group => new { ProjectId = group.Key, Count = group.Count() })
        .ToDictionaryAsync(item => item.ProjectId, item => item.Count, cancellationToken);

    return Results.Ok(projects.Select(project => ToProjectResponse(
        project,
        ticketCounts.GetValueOrDefault(project.Id),
        customerCounts.GetValueOrDefault(project.Id),
        project.ProjectMembers.Select(x => x.MemberId).ToList())).ToList());
})
.Produces<List<ProjectResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("ListProjects")
.WithTags("Projects");

app.MapPatch("/api/teams/{teamId:guid}/projects/{projectId:guid}", async (
    HttpContext httpContext,
    Guid teamId,
    Guid projectId,
    UpdateProjectRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var project = await dbContext.Projects
        .Include(x => x.ProjectMembers)
        .FirstOrDefaultAsync(x => x.Id == projectId && x.TeamId == teamId, cancellationToken);

    if (project is null)
    {
        return NotFoundError("project was not found", "project_not_found");
    }

    var projectName = request.Name.Trim();
    if (string.IsNullOrWhiteSpace(projectName))
    {
        return Results.BadRequest(new ApiErrorResponse("name is required"));
    }

    Member? leadMember = null;
    if (request.LeadMemberId.HasValue)
    {
        leadMember = await dbContext.Members.FirstOrDefaultAsync(
            member => member.Id == request.LeadMemberId.Value && member.TeamId == teamId,
            cancellationToken);

        if (leadMember is null)
        {
            return Results.BadRequest(new ApiErrorResponse("leadMemberId does not belong to the team"));
        }
    }

    var participantMemberIds = (request.ParticipantMemberIds ?? [])
        .Where(id => id != Guid.Empty)
        .Distinct()
        .ToList();
    if (leadMember is not null && !participantMemberIds.Contains(leadMember.Id))
    {
        participantMemberIds.Insert(0, leadMember.Id);
    }

    if (participantMemberIds.Count > 0)
    {
        var validParticipantIds = await dbContext.Members
            .Where(member => member.TeamId == teamId && participantMemberIds.Contains(member.Id))
            .Select(member => member.Id)
            .ToListAsync(cancellationToken);

        if (validParticipantIds.Count != participantMemberIds.Count)
        {
            return Results.BadRequest(new ApiErrorResponse("participantMemberIds contains members outside the team"));
        }
    }

    project.Name = projectName;
    project.Description = TrimToNullable(request.Description, 2048);
    project.StageLabel = TrimToNullable(request.StageLabel, 128);
    project.Summary = TrimToNullable(request.Summary, 4096);
    project.RiskSummary = TrimToNullable(request.RiskSummary, 4096);
    project.NextSteps = TrimToNullable(request.NextSteps, 4096);
    project.LeadMemberId = leadMember?.Id;
    project.UpdatedAt = DateTimeOffset.UtcNow;

    var existingParticipantIds = project.ProjectMembers.Select(x => x.MemberId).ToHashSet();
    var removeMemberships = project.ProjectMembers
        .Where(x => !participantMemberIds.Contains(x.MemberId))
        .ToList();
    if (removeMemberships.Count > 0)
    {
        dbContext.ProjectMembers.RemoveRange(removeMemberships);
    }

    foreach (var participantMemberId in participantMemberIds.Where(id => !existingParticipantIds.Contains(id)))
    {
        dbContext.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            MemberId = participantMemberId,
            RoleLabel = participantMemberId == project.LeadMemberId ? "负责人" : "参与成员",
        });
    }

    foreach (var membership in project.ProjectMembers.Where(x => participantMemberIds.Contains(x.MemberId)))
    {
        membership.RoleLabel = membership.MemberId == project.LeadMemberId ? "负责人" : "参与成员";
        membership.UpdatedAt = DateTimeOffset.UtcNow;
    }

    await dbContext.SaveChangesAsync(cancellationToken);

    var ticketCount = await dbContext.Tickets.CountAsync(ticket => ticket.ProjectId == project.Id, cancellationToken);
    var customerCount = await dbContext.Customers.CountAsync(
        customer => customer.ProjectId == project.Id,
        cancellationToken);

    return Results.Ok(ToProjectResponse(project, ticketCount, customerCount, participantMemberIds));
})
.Produces<ProjectResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("UpdateProject")
.WithTags("Projects");

app.MapPost("/api/teams/{teamId:guid}/concierge-apps", async (
    HttpContext httpContext,
    Guid teamId,
    CreateConciergeAppRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var project = await dbContext.Projects
        .FirstOrDefaultAsync(x => x.Id == request.ProjectId && x.TeamId == teamId, cancellationToken);

    if (project is null)
    {
        return Results.BadRequest(new ApiErrorResponse("projectId does not belong to the team"));
    }

    Member? primaryAiMember = null;
    if (request.PrimaryAiMemberId.HasValue)
    {
        primaryAiMember = await dbContext.Members
            .FirstOrDefaultAsync(
                member => member.Id == request.PrimaryAiMemberId.Value
                    && member.TeamId == teamId
                    && member.MemberType == MemberType.Ai,
                cancellationToken);

        if (primaryAiMember is null)
        {
            return Results.BadRequest(new ApiErrorResponse("primaryAiMemberId must be an AI member in the team"));
        }
    }

    var name = request.Name.Trim();
    if (string.IsNullOrWhiteSpace(name))
    {
        return Results.BadRequest(new ApiErrorResponse("name is required"));
    }

    var conciergeApp = new ConciergeApp
    {
        TeamId = teamId,
        ProjectId = project.Id,
        Name = name,
        Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
        ServiceScope = string.IsNullOrWhiteSpace(request.ServiceScope) ? null : request.ServiceScope.Trim(),
        WelcomeMessage = string.IsNullOrWhiteSpace(request.WelcomeMessage) ? null : request.WelcomeMessage.Trim(),
        FaqScope = string.IsNullOrWhiteSpace(request.FaqScope) ? null : request.FaqScope.Trim(),
        BusinessHours = string.IsNullOrWhiteSpace(request.BusinessHours) ? null : request.BusinessHours.Trim(),
        ChannelLabel = string.IsNullOrWhiteSpace(request.ChannelLabel) ? null : request.ChannelLabel.Trim(),
        Status = ConciergeAppStatus.Draft,
        PrimaryAiMemberId = primaryAiMember?.Id,
        TicketCreationPolicy = string.IsNullOrWhiteSpace(request.TicketCreationPolicy) ? null : request.TicketCreationPolicy.Trim(),
        HumanHandoffPolicy = string.IsNullOrWhiteSpace(request.HumanHandoffPolicy) ? null : request.HumanHandoffPolicy.Trim(),
    };

    dbContext.ConciergeApps.Add(conciergeApp);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created(
        $"/api/teams/{teamId}/concierge-apps/{conciergeApp.Id}",
        ToConciergeAppResponse(conciergeApp));
})
.Produces<ConciergeAppResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.WithName("CreateConciergeApp")
.WithTags("ConciergeApps");

app.MapGet("/api/teams/{teamId:guid}/concierge-apps", async (
    HttpContext httpContext,
    Guid teamId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
    if (!teamExists)
    {
        return Results.NotFound(new ApiErrorResponse("team was not found"));
    }

    var apps = await dbContext.ConciergeApps
        .Where(appEntity => appEntity.TeamId == teamId)
        .OrderBy(appEntity => appEntity.CreatedAt)
        .ToListAsync(cancellationToken);

    return Results.Ok(apps.Select(ToConciergeAppResponse).ToList());
})
.Produces<List<ConciergeAppResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("ListConciergeApps")
.WithTags("ConciergeApps");

app.MapPatch("/api/teams/{teamId:guid}/concierge-apps/{conciergeAppId:guid}", async (
    HttpContext httpContext,
    Guid teamId,
    Guid conciergeAppId,
    UpdateConciergeAppRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var conciergeApp = await dbContext.ConciergeApps
        .FirstOrDefaultAsync(x => x.Id == conciergeAppId && x.TeamId == teamId, cancellationToken);

    if (conciergeApp is null)
    {
        return NotFoundError("concierge app was not found", "concierge_app_not_found");
    }

    var primaryAiMemberId = request.PrimaryAiMemberId;
    if (primaryAiMemberId.HasValue)
    {
        var primaryAiMemberExists = await dbContext.Members
            .AnyAsync(
                member => member.Id == primaryAiMemberId.Value
                    && member.TeamId == teamId
                    && member.MemberType == MemberType.Ai,
                cancellationToken);

        if (!primaryAiMemberExists)
        {
            return BadRequestError("primaryAiMemberId must be an AI member in the team", "invalid_primary_ai_member");
        }
    }

    var name = request.Name.Trim();
    if (string.IsNullOrWhiteSpace(name))
    {
        return BadRequestError("name is required", "concierge_name_required");
    }

    conciergeApp.Name = name;
    conciergeApp.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
    conciergeApp.ServiceScope = string.IsNullOrWhiteSpace(request.ServiceScope) ? null : request.ServiceScope.Trim();
    conciergeApp.WelcomeMessage = string.IsNullOrWhiteSpace(request.WelcomeMessage) ? null : request.WelcomeMessage.Trim();
    conciergeApp.FaqScope = string.IsNullOrWhiteSpace(request.FaqScope) ? null : request.FaqScope.Trim();
    conciergeApp.BusinessHours = string.IsNullOrWhiteSpace(request.BusinessHours) ? null : request.BusinessHours.Trim();
    conciergeApp.ChannelLabel = string.IsNullOrWhiteSpace(request.ChannelLabel) ? null : request.ChannelLabel.Trim();
    conciergeApp.Status = request.Status;
    conciergeApp.PrimaryAiMemberId = primaryAiMemberId;
    conciergeApp.TicketCreationPolicy = string.IsNullOrWhiteSpace(request.TicketCreationPolicy) ? null : request.TicketCreationPolicy.Trim();
    conciergeApp.HumanHandoffPolicy = string.IsNullOrWhiteSpace(request.HumanHandoffPolicy) ? null : request.HumanHandoffPolicy.Trim();
    conciergeApp.UpdatedAt = DateTimeOffset.UtcNow;

    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Ok(ToConciergeAppResponse(conciergeApp));
})
.Produces<ConciergeAppResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("UpdateConciergeApp")
.WithTags("ConciergeApps");

app.MapGet("/api/public/concierge-apps/{conciergeAppId:guid}", async (
    Guid conciergeAppId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var conciergeApp = await dbContext.ConciergeApps
        .Include(x => x.Team)
        .Include(x => x.Project)
        .FirstOrDefaultAsync(x => x.Id == conciergeAppId, cancellationToken);

    if (conciergeApp is null || conciergeApp.Status == ConciergeAppStatus.Archived)
    {
        return NotFoundError("concierge app was not found", "concierge_app_not_found");
    }

    return Results.Ok(new PublicConciergeAppResponse(
        conciergeApp.Id,
        conciergeApp.Name,
        conciergeApp.Description,
        conciergeApp.ServiceScope,
        conciergeApp.WelcomeMessage,
        conciergeApp.FaqScope,
        conciergeApp.BusinessHours,
        conciergeApp.ChannelLabel,
        conciergeApp.Status,
        conciergeApp.Team?.BrandName,
        conciergeApp.Project?.Name));
})
.Produces<PublicConciergeAppResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("GetPublicConciergeApp")
.WithTags("PublicConcierge");

app.MapPost("/api/public/concierge-apps/{conciergeAppId:guid}/intake", async (
    Guid conciergeAppId,
    PublicConciergeIntakeRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var conciergeApp = await dbContext.ConciergeApps
        .Include(x => x.Team)
        .FirstOrDefaultAsync(x => x.Id == conciergeAppId, cancellationToken);

    if (conciergeApp is null || conciergeApp.Status == ConciergeAppStatus.Archived)
    {
        return NotFoundError("concierge app was not found", "concierge_app_not_found");
    }

    if (conciergeApp.Status == ConciergeAppStatus.Inactive)
    {
        return BadRequestError("concierge app is inactive", "concierge_app_inactive");
    }

    var displayName = request.DisplayName.Trim();
    var message = request.Message.Trim();
    var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
    var companyName = string.IsNullOrWhiteSpace(request.CompanyName) ? null : request.CompanyName.Trim();

    if (string.IsNullOrWhiteSpace(displayName))
    {
        return BadRequestError("displayName is required", "display_name_required");
    }

    if (string.IsNullOrWhiteSpace(message))
    {
        return BadRequestError("message is required", "message_required");
    }

    if (email is not null && !IsValidEmail(email))
    {
        return BadRequestError("email format is invalid", "invalid_email");
    }

    Customer? customer = null;
    if (email is not null)
    {
        customer = await dbContext.Customers
            .FirstOrDefaultAsync(x => x.TeamId == conciergeApp.TeamId && x.Email == email, cancellationToken);
    }

    if (customer is null)
    {
        customer = new Customer
        {
            TeamId = conciergeApp.TeamId,
            DisplayName = displayName,
            Email = email,
            CompanyName = companyName,
            SourceLabel = conciergeApp.ChannelLabel,
            Status = email is null ? CustomerStatus.Anonymous : CustomerStatus.Active,
        };
        dbContext.Customers.Add(customer);
    }
    else
    {
        customer.DisplayName = displayName;
        customer.CompanyName = companyName ?? customer.CompanyName;
        customer.SourceLabel ??= conciergeApp.ChannelLabel;
        customer.UpdatedAt = DateTimeOffset.UtcNow;
    }

    var conversation = new Conversation
    {
        TeamId = conciergeApp.TeamId,
        ConciergeAppId = conciergeApp.Id,
        Customer = customer,
        Status = ConversationStatus.Open,
        Messages =
        [
            new ConversationMessage
            {
                ParticipantType = ConversationParticipantType.Customer,
                Content = message,
                SenderName = displayName,
            }
        ]
    };

    dbContext.Conversations.Add(conversation);

    Ticket? ticket = null;
    if (request.AutoCreateTicket)
    {
        ticket = new Ticket
        {
            TeamId = conciergeApp.TeamId,
            ProjectId = conciergeApp.ProjectId,
            ConciergeAppId = conciergeApp.Id,
            Customer = customer,
            Conversation = conversation,
            Title = BuildTicketTitle(message),
            Summary = message,
            Status = TicketStatus.Pending,
            Priority = request.AutoTicketPriority,
        };
        dbContext.Tickets.Add(ticket);
    }

    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created(
        $"/api/public/concierge-apps/{conciergeAppId}/intake/{conversation.Id}",
        new PublicConciergeIntakeResponse(
            customer.Id,
            conversation.Id,
            ticket?.Id,
            "已收到你的需求，我们会尽快跟进。"));
})
.Produces<PublicConciergeIntakeResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("CreatePublicConciergeIntake")
.WithTags("PublicConcierge");

app.MapPost("/api/teams/{teamId:guid}/customers", async (
    HttpContext httpContext,
    Guid teamId,
    CreateCustomerRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
    if (!teamExists)
    {
        return Results.NotFound(new ApiErrorResponse("team was not found"));
    }

    var displayName = request.DisplayName.Trim();
    if (string.IsNullOrWhiteSpace(displayName))
    {
        return Results.BadRequest(new ApiErrorResponse("displayName is required"));
    }

    var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
    if (email is not null && !IsValidEmail(email))
    {
        return BadRequestError("email format is invalid", "invalid_email");
    }

    if (!string.IsNullOrWhiteSpace(email))
    {
        var existingCustomer = await dbContext.Customers
            .FirstOrDefaultAsync(customer => customer.TeamId == teamId && customer.Email == email, cancellationToken);

        if (existingCustomer is not null)
        {
            return ConflictError("customer email already exists in the team", "customer_email_exists");
        }
    }

    if (request.ProjectId.HasValue)
    {
        var projectExists = await dbContext.Projects
            .AnyAsync(project => project.Id == request.ProjectId.Value && project.TeamId == teamId, cancellationToken);

        if (!projectExists)
        {
            return BadRequestError("projectId does not belong to the team", "invalid_project_id");
        }
    }

    var customer = new Customer
    {
        TeamId = teamId,
        DisplayName = displayName,
        Email = email,
        PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
        CompanyName = string.IsNullOrWhiteSpace(request.CompanyName) ? null : request.CompanyName.Trim(),
        SourceLabel = string.IsNullOrWhiteSpace(request.SourceLabel) ? null : request.SourceLabel.Trim(),
        Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags.Trim(),
        FollowUpStatus = request.FollowUpStatus,
        LastContactedAt = request.LastContactedAt,
        ProjectId = request.ProjectId,
        Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
        Status = email is null ? CustomerStatus.Anonymous : CustomerStatus.Active,
    };

    dbContext.Customers.Add(customer);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/teams/{teamId}/customers/{customer.Id}", ToCustomerResponse(customer));
})
.Produces<CustomerResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
.WithName("CreateCustomer")
.WithTags("Customers");

app.MapPatch("/api/teams/{teamId:guid}/customers/{customerId:guid}", async (
    HttpContext httpContext,
    Guid teamId,
    Guid customerId,
    UpdateCustomerRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var customer = await dbContext.Customers
        .FirstOrDefaultAsync(x => x.Id == customerId && x.TeamId == teamId, cancellationToken);

    if (customer is null)
    {
        return NotFoundError("customer was not found", "customer_not_found");
    }

    var displayName = request.DisplayName.Trim();
    if (string.IsNullOrWhiteSpace(displayName))
    {
        return BadRequestError("displayName is required", "display_name_required");
    }

    var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
    if (email is not null && !IsValidEmail(email))
    {
        return BadRequestError("email format is invalid", "invalid_email");
    }

    if (!string.IsNullOrWhiteSpace(email))
    {
        var emailConflict = await dbContext.Customers
            .AnyAsync(x => x.TeamId == teamId && x.Email == email && x.Id != customerId, cancellationToken);

        if (emailConflict)
        {
            return ConflictError("customer email already exists in the team", "customer_email_exists");
        }
    }

    if (request.ProjectId.HasValue)
    {
        var projectExists = await dbContext.Projects
            .AnyAsync(project => project.Id == request.ProjectId.Value && project.TeamId == teamId, cancellationToken);

        if (!projectExists)
        {
            return BadRequestError("projectId does not belong to the team", "invalid_project_id");
        }
    }

    customer.DisplayName = displayName;
    customer.Email = email;
    customer.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
    customer.CompanyName = string.IsNullOrWhiteSpace(request.CompanyName) ? null : request.CompanyName.Trim();
    customer.SourceLabel = string.IsNullOrWhiteSpace(request.SourceLabel) ? null : request.SourceLabel.Trim();
    customer.Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags.Trim();
    customer.FollowUpStatus = request.FollowUpStatus;
    customer.LastContactedAt = request.LastContactedAt;
    customer.ProjectId = request.ProjectId;
    customer.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
    customer.Status = request.Status;
    customer.UpdatedAt = DateTimeOffset.UtcNow;

    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Ok(ToCustomerResponse(customer));
})
.Produces<CustomerResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
.WithName("UpdateCustomer")
.WithTags("Customers");

app.MapGet("/api/teams/{teamId:guid}/customers", async (
    HttpContext httpContext,
    Guid teamId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
    if (!teamExists)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var customers = await dbContext.Customers
        .Where(customer => customer.TeamId == teamId)
        .OrderBy(customer => customer.CreatedAt)
        .ToListAsync(cancellationToken);

    return Results.Ok(customers.Select(ToCustomerResponse).ToList());
})
.Produces<List<CustomerResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("ListCustomers")
.WithTags("Customers");

app.MapPost("/api/concierge-apps/{conciergeAppId:guid}/conversations", async (
    HttpContext httpContext,
    Guid conciergeAppId,
    CreateConversationRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var conciergeApp = await dbContext.ConciergeApps
        .FirstOrDefaultAsync(x => x.Id == conciergeAppId, cancellationToken);

    if (conciergeApp is null)
    {
        return Results.NotFound(new ApiErrorResponse("concierge app was not found"));
    }

    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, conciergeApp.TeamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var initialMessage = request.InitialMessage.Trim();
    if (string.IsNullOrWhiteSpace(initialMessage))
    {
        return BadRequestError("initialMessage is required", "initial_message_required");
    }

    Customer? customer = null;
    if (request.CustomerId.HasValue)
    {
        customer = await dbContext.Customers
            .FirstOrDefaultAsync(
                x => x.Id == request.CustomerId.Value && x.TeamId == conciergeApp.TeamId,
                cancellationToken);

        if (customer is null)
        {
            return Results.BadRequest(new ApiErrorResponse("customerId does not belong to the concierge app team"));
        }
    }
    else if (!string.IsNullOrWhiteSpace(request.CustomerDisplayName))
    {
        var displayName = request.CustomerDisplayName.Trim();
        var email = string.IsNullOrWhiteSpace(request.CustomerEmail) ? null : request.CustomerEmail.Trim();

        if (email is not null && !IsValidEmail(email))
        {
            return BadRequestError("customerEmail format is invalid", "invalid_email");
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            customer = await dbContext.Customers
                .FirstOrDefaultAsync(
                    x => x.TeamId == conciergeApp.TeamId && x.Email == email,
                    cancellationToken);
        }

        if (customer is null)
        {
            customer = new Customer
            {
                TeamId = conciergeApp.TeamId,
                DisplayName = displayName,
                Email = email,
                Status = email is null ? CustomerStatus.Anonymous : CustomerStatus.Active,
            };
            dbContext.Customers.Add(customer);
        }
    }

    var conversation = new Conversation
    {
        TeamId = conciergeApp.TeamId,
        ConciergeAppId = conciergeApp.Id,
        Customer = customer,
        Status = ConversationStatus.Open,
        Messages =
        [
            new ConversationMessage
            {
                ParticipantType = ConversationParticipantType.Customer,
                SenderName = customer?.DisplayName,
                Content = initialMessage,
            }
        ]
    };

    dbContext.Conversations.Add(conversation);

    Ticket? ticket = null;
    if (request.AutoCreateTicket)
    {
        var title = BuildTicketTitle(initialMessage);
        ticket = new Ticket
        {
            TeamId = conciergeApp.TeamId,
            ProjectId = conciergeApp.ProjectId,
            ConciergeAppId = conciergeApp.Id,
            Customer = customer,
            Conversation = conversation,
            Title = title,
            Summary = initialMessage,
            Category = "客户咨询",
            Status = TicketStatus.Pending,
            Priority = request.AutoTicketPriority,
            LastActivityAt = DateTimeOffset.UtcNow,
        };

        dbContext.Tickets.Add(ticket);
        dbContext.TicketActivities.Add(new TicketActivity
        {
            TeamId = conciergeApp.TeamId,
            Ticket = ticket,
            ActivityType = TicketActivityType.Created,
            Summary = "自动从会话创建工单",
            Detail = initialMessage,
        });
    }

    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created(
        $"/api/conversations/{conversation.Id}",
        new ConversationResponse(
            conversation.Id,
            conversation.TeamId,
            conversation.ConciergeAppId,
            conversation.CustomerId,
            conversation.Status,
            initialMessage,
            ticket?.Id));
})
.Produces<ConversationResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("CreateConversation")
.WithTags("Conversations");

app.MapGet("/api/concierge-apps/{conciergeAppId:guid}/conversations", async (
    HttpContext httpContext,
    Guid conciergeAppId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var conciergeApp = await dbContext.ConciergeApps
        .FirstOrDefaultAsync(x => x.Id == conciergeAppId, cancellationToken);
    if (conciergeApp is null)
    {
        return Results.NotFound(new ApiErrorResponse("concierge app was not found"));
    }

    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, conciergeApp.TeamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var conversations = await dbContext.Conversations
        .Where(conversation => conversation.ConciergeAppId == conciergeAppId)
        .Include(conversation => conversation.Customer)
        .Include(conversation => conversation.Messages)
        .OrderByDescending(conversation => conversation.CreatedAt)
        .ToListAsync(cancellationToken);

    var result = conversations.Select(conversation => new ConversationSummaryResponse(
        conversation.Id,
        conversation.TeamId,
        conversation.ConciergeAppId,
        conversation.CustomerId,
        conversation.Customer?.DisplayName,
        conversation.Status,
        conversation.Messages.Count,
        conversation.Messages
            .OrderByDescending(message => message.CreatedAt)
            .Select(message => message.Content)
            .FirstOrDefault(),
        conversation.CreatedAt)).ToList();

    return Results.Ok(result);
})
.Produces<List<ConversationSummaryResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("ListConversations")
.WithTags("Conversations");

app.MapGet("/api/conversations/{conversationId:guid}", async (
    HttpContext httpContext,
    Guid conversationId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var conversation = await dbContext.Conversations
        .Include(x => x.Customer)
        .Include(x => x.Messages)
        .Include(x => x.Tickets)
        .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

    if (conversation is null)
    {
        return Results.NotFound(new ApiErrorResponse("conversation was not found"));
    }

    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, conversation.TeamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    return Results.Ok(new ConversationDetailResponse(
        conversation.Id,
        conversation.TeamId,
        conversation.ConciergeAppId,
        conversation.CustomerId,
        conversation.Customer is null
            ? null
            : new ConversationCustomerResponse(
                conversation.Customer.DisplayName,
                conversation.Customer.Email),
        conversation.Status,
        conversation.Messages
            .OrderBy(message => message.CreatedAt)
            .Select(message => new ConversationMessageResponse(
                message.Id,
                message.ParticipantType,
                message.MemberId,
                message.SenderName,
                message.Content,
                message.CreatedAt))
            .ToList(),
        conversation.Tickets.Select(ticket => ticket.Id).ToList()));
})
.Produces<ConversationDetailResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("GetConversation")
.WithTags("Conversations");

app.MapPost("/api/conversations/{conversationId:guid}/tickets", async (
    HttpContext httpContext,
    Guid conversationId,
    CreateTicketRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var conversation = await dbContext.Conversations
        .Include(x => x.ConciergeApp)
        .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

    if (conversation is null)
    {
        return Results.NotFound(new ApiErrorResponse("conversation was not found"));
    }

    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, conversation.TeamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var title = request.Title.Trim();
    var summary = request.Summary.Trim();
    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(summary))
    {
        return BadRequestError("title and summary are required", "ticket_required_fields");
    }

    Member? assignedMember = null;
    if (request.AssignedMemberId.HasValue)
    {
        assignedMember = await dbContext.Members
            .FirstOrDefaultAsync(
                member => member.Id == request.AssignedMemberId.Value && member.TeamId == conversation.TeamId,
                cancellationToken);

        if (assignedMember is null)
        {
            return Results.BadRequest(new ApiErrorResponse("assignedMemberId does not belong to the conversation team"));
        }
    }

    var ticket = new Ticket
    {
        TeamId = conversation.TeamId,
        ProjectId = conversation.ConciergeApp!.ProjectId,
        ConciergeAppId = conversation.ConciergeAppId,
        CustomerId = conversation.CustomerId,
        ConversationId = conversation.Id,
        Title = title,
        Summary = summary,
        Category = "客户咨询",
        Status = TicketStatus.Pending,
        Priority = request.Priority,
        AssignedMemberId = assignedMember?.Id,
        LastActivityAt = DateTimeOffset.UtcNow,
    };

    dbContext.Tickets.Add(ticket);
    dbContext.TicketActivities.Add(new TicketActivity
    {
        TeamId = conversation.TeamId,
        Ticket = ticket,
        ActorMemberId = assignedMember?.Id,
        ActivityType = TicketActivityType.Created,
        Summary = "手动创建工单",
        Detail = summary,
    });
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/teams/{conversation.TeamId}/tickets/{ticket.Id}", ToTicketResponse(ticket));
})
.Produces<TicketResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("CreateTicket")
.WithTags("Tickets");

app.MapGet("/api/teams/{teamId:guid}/tickets", async (
    HttpContext httpContext,
    Guid teamId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
    if (!teamExists)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var tickets = await dbContext.Tickets
        .Where(ticket => ticket.TeamId == teamId)
        .Include(ticket => ticket.Customer)
        .Include(ticket => ticket.AssignedMember)
        .OrderByDescending(ticket => ticket.CreatedAt)
        .ToListAsync(cancellationToken);

    var result = tickets.Select(ToTicketResponse).ToList();

    return Results.Ok(result);
})
.Produces<List<TicketResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("ListTickets")
.WithTags("Tickets");

app.MapGet("/api/teams/{teamId:guid}/tickets/{ticketId:guid}", async (
    HttpContext httpContext,
    Guid teamId,
    Guid ticketId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var ticket = await dbContext.Tickets
        .Where(x => x.TeamId == teamId && x.Id == ticketId)
        .Include(x => x.Customer)
        .Include(x => x.AssignedMember)
        .Include(x => x.Activities)
            .ThenInclude(x => x.ActorMember)
        .Include(x => x.Activities)
            .ThenInclude(x => x.ActorUser)
        .FirstOrDefaultAsync(cancellationToken);

    if (ticket is null)
    {
        return NotFoundError("ticket was not found", "ticket_not_found");
    }

    return Results.Ok(ToTicketDetailResponse(ticket));
})
.Produces<TicketDetailResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("GetTicket")
.WithTags("Tickets");

app.MapPost("/api/teams/{teamId:guid}/tickets/{ticketId:guid}/workflows", async (
    HttpContext httpContext,
    Guid teamId,
    Guid ticketId,
    RunTicketWorkflowRequest request,
    AppDbContext dbContext,
    AuditLogService auditLogService,
    AgentWorkflowOrchestrator orchestrator,
    AgentWorkflowExecutionService executionService,
    AgentWorkflowWritebackService writebackService,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    var team = await dbContext.Teams.FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);
    if (team is null)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var ticket = await dbContext.Tickets
        .Include(x => x.Team)
            .ThenInclude(x => x!.Members)
                .ThenInclude(x => x.AiProfile)
        .Include(x => x.Customer)
        .Include(x => x.Project)
        .Include(x => x.Conversation)
            .ThenInclude(x => x!.Messages)
        .Include(x => x.AssignedMember)
        .FirstOrDefaultAsync(x => x.Id == ticketId && x.TeamId == teamId, cancellationToken);
    if (ticket is null)
    {
        return NotFoundError("ticket was not found", "ticket_not_found");
    }

    var aiMembers = await dbContext.Members
        .Include(x => x.AiProfile)
        .Where(x =>
            x.TeamId == teamId
            && x.MemberType == MemberType.Ai
            && x.Status == MemberStatus.Active)
        .OrderBy(x => x.DisplayName)
        .ToListAsync(cancellationToken);

    Member? startedByMember = null;
    if (request.StartedByMemberId.HasValue)
    {
        startedByMember = aiMembers.FirstOrDefault(x => x.Id == request.StartedByMemberId.Value);
        if (startedByMember is null)
        {
            return BadRequestError("startedByMemberId must reference an active AI member in the team", "invalid_started_by_member");
        }
    }

    var workflow = orchestrator.CreateTicketWorkflow(
        team,
        ticket,
        aiMembers,
        session?.UserId,
        startedByMember?.Id,
        request.Goal);

    var integrationConnections = await dbContext.IntegrationConnections
        .Where(x => x.TeamId == teamId)
        .OrderBy(x => x.ExternalSystemType)
        .ThenBy(x => x.Name)
        .ToListAsync(cancellationToken);

    await executionService.EnrichAsync(workflow, ticket, integrationConnections, cancellationToken);
    await writebackService.ApplyAsync(workflow, ticket, session, cancellationToken);

    var coordinatorStep = workflow.Steps.FirstOrDefault(x => x.ActionType == "ticket-coordinator");
    if (ticket.Status == TicketStatus.Pending)
    {
        ticket.Status = TicketStatus.InProgress;
    }

    if (ticket.AssignedMemberId is null && coordinatorStep?.MemberId is not null)
    {
        ticket.AssignedMemberId = coordinatorStep.MemberId;
    }

    dbContext.AgentWorkflowRuns.Add(workflow);
    await dbContext.SaveChangesAsync(cancellationToken);
    await auditLogService.WriteAsync(
        dbContext,
        httpContext,
        "workflow.run",
        "agent_workflow_run",
        workflow.Id,
        $"Workflow started for ticket {ticket.Title}.",
        session?.UserId,
        teamId,
        "success",
        cancellationToken);

    var persistedWorkflow = await dbContext.AgentWorkflowRuns
        .Include(x => x.StartedByMember)
        .Include(x => x.Steps)
            .ThenInclude(x => x.Member)
                .ThenInclude(x => x!.AiProfile)
        .Include(x => x.Steps)
            .ThenInclude(x => x.ExecutionLogs)
                .ThenInclude(x => x.Member)
        .Include(x => x.Steps)
            .ThenInclude(x => x.HandoffToMember)
                .ThenInclude(x => x!.AiProfile)
        .FirstAsync(x => x.Id == workflow.Id, cancellationToken);

    return Results.Created(
        $"/api/teams/{teamId}/workflows/{workflow.Id}",
        ToAgentWorkflowResponse(persistedWorkflow));
})
.Produces<AgentWorkflowResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("RunTicketWorkflow")
.WithTags("Workflows");

app.MapPost("/api/teams/{teamId:guid}/conversations/{conversationId:guid}/workflows", async (
    HttpContext httpContext,
    Guid teamId,
    Guid conversationId,
    RunAgentWorkflowRequest request,
    AppDbContext dbContext,
    AuditLogService auditLogService,
    AgentWorkflowOrchestrator orchestrator,
    AgentWorkflowExecutionService executionService,
    AgentWorkflowWritebackService writebackService,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    var team = await dbContext.Teams.FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);
    if (team is null)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var conversation = await dbContext.Conversations
        .Include(x => x.Team)
            .ThenInclude(x => x!.Members)
                .ThenInclude(x => x.AiProfile)
        .Include(x => x.Customer)
        .Include(x => x.ConciergeApp)
            .ThenInclude(x => x!.Project)
        .Include(x => x.Messages)
        .FirstOrDefaultAsync(x => x.Id == conversationId && x.TeamId == teamId, cancellationToken);
    if (conversation is null)
    {
        return NotFoundError("conversation was not found", "conversation_not_found");
    }

    var aiMembers = await dbContext.Members
        .Include(x => x.AiProfile)
        .Where(x => x.TeamId == teamId && x.MemberType == MemberType.Ai && x.Status == MemberStatus.Active)
        .OrderBy(x => x.DisplayName)
        .ToListAsync(cancellationToken);

    Member? startedByMember = null;
    if (request.StartedByMemberId.HasValue)
    {
        startedByMember = aiMembers.FirstOrDefault(x => x.Id == request.StartedByMemberId.Value);
        if (startedByMember is null)
        {
            return BadRequestError("startedByMemberId must reference an active AI member in the team", "invalid_started_by_member");
        }
    }

    var workflow = orchestrator.CreateConversationWorkflow(
        team,
        conversation,
        aiMembers,
        session?.UserId,
        startedByMember?.Id,
        request.Goal);

    var integrationConnections = await dbContext.IntegrationConnections
        .Where(x => x.TeamId == teamId)
        .OrderBy(x => x.ExternalSystemType)
        .ThenBy(x => x.Name)
        .ToListAsync(cancellationToken);

    await executionService.EnrichAsync(workflow, conversation, integrationConnections, cancellationToken);
    await writebackService.ApplyAsync(workflow, conversation, session, cancellationToken);

    dbContext.AgentWorkflowRuns.Add(workflow);
    await dbContext.SaveChangesAsync(cancellationToken);
    await auditLogService.WriteAsync(
        dbContext,
        httpContext,
        "workflow.run",
        "agent_workflow_run",
        workflow.Id,
        $"Workflow started for conversation {conversation.Id}.",
        session?.UserId,
        teamId,
        "success",
        cancellationToken);

    var persistedWorkflow = await dbContext.AgentWorkflowRuns
        .Include(x => x.StartedByMember)
        .Include(x => x.Steps)
            .ThenInclude(x => x.Member)
                .ThenInclude(x => x!.AiProfile)
        .Include(x => x.Steps)
            .ThenInclude(x => x.ExecutionLogs)
                .ThenInclude(x => x.Member)
        .Include(x => x.Steps)
            .ThenInclude(x => x.HandoffToMember)
                .ThenInclude(x => x!.AiProfile)
        .FirstAsync(x => x.Id == workflow.Id, cancellationToken);

    return Results.Created(
        $"/api/teams/{teamId}/workflows/{workflow.Id}",
        ToAgentWorkflowResponse(persistedWorkflow));
})
.Produces<AgentWorkflowResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("RunConversationWorkflow")
.WithTags("Workflows");

app.MapPost("/api/teams/{teamId:guid}/projects/{projectId:guid}/workflows", async (
    HttpContext httpContext,
    Guid teamId,
    Guid projectId,
    RunAgentWorkflowRequest request,
    AppDbContext dbContext,
    AuditLogService auditLogService,
    AgentWorkflowOrchestrator orchestrator,
    AgentWorkflowExecutionService executionService,
    AgentWorkflowWritebackService writebackService,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    var team = await dbContext.Teams.FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);
    if (team is null)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var project = await dbContext.Projects
        .FirstOrDefaultAsync(x => x.Id == projectId && x.TeamId == teamId, cancellationToken);
    if (project is null)
    {
        return NotFoundError("project was not found", "project_not_found");
    }

    var aiMembers = await dbContext.Members
        .Include(x => x.AiProfile)
        .Where(x => x.TeamId == teamId && x.MemberType == MemberType.Ai && x.Status == MemberStatus.Active)
        .OrderBy(x => x.DisplayName)
        .ToListAsync(cancellationToken);

    Member? startedByMember = null;
    if (request.StartedByMemberId.HasValue)
    {
        startedByMember = aiMembers.FirstOrDefault(x => x.Id == request.StartedByMemberId.Value);
        if (startedByMember is null)
        {
            return BadRequestError("startedByMemberId must reference an active AI member in the team", "invalid_started_by_member");
        }
    }

    var workflow = orchestrator.CreateProjectWorkflow(
        team,
        project,
        aiMembers,
        session?.UserId,
        startedByMember?.Id,
        request.Goal);

    var integrationConnections = await dbContext.IntegrationConnections
        .Where(x => x.TeamId == teamId)
        .OrderBy(x => x.ExternalSystemType)
        .ThenBy(x => x.Name)
        .ToListAsync(cancellationToken);

    await executionService.EnrichAsync(workflow, project, integrationConnections, cancellationToken);
    await writebackService.ApplyAsync(workflow, project, cancellationToken);

    dbContext.AgentWorkflowRuns.Add(workflow);
    await dbContext.SaveChangesAsync(cancellationToken);
    await auditLogService.WriteAsync(
        dbContext,
        httpContext,
        "workflow.run",
        "agent_workflow_run",
        workflow.Id,
        $"Workflow started for project {project.Name}.",
        session?.UserId,
        teamId,
        "success",
        cancellationToken);

    var persistedWorkflow = await dbContext.AgentWorkflowRuns
        .Include(x => x.StartedByMember)
        .Include(x => x.Steps)
            .ThenInclude(x => x.Member)
                .ThenInclude(x => x!.AiProfile)
        .Include(x => x.Steps)
            .ThenInclude(x => x.ExecutionLogs)
                .ThenInclude(x => x.Member)
        .Include(x => x.Steps)
            .ThenInclude(x => x.HandoffToMember)
                .ThenInclude(x => x!.AiProfile)
        .FirstAsync(x => x.Id == workflow.Id, cancellationToken);

    return Results.Created(
        $"/api/teams/{teamId}/workflows/{workflow.Id}",
        ToAgentWorkflowResponse(persistedWorkflow));
})
.Produces<AgentWorkflowResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("RunProjectWorkflow")
.WithTags("Workflows");

app.MapGet("/api/teams/{teamId:guid}/workflows", async (
    HttpContext httpContext,
    Guid teamId,
    Guid? ticketId,
    Guid? conversationId,
    Guid? projectId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var query = dbContext.AgentWorkflowRuns
        .Include(x => x.StartedByMember)
        .Include(x => x.Steps)
            .ThenInclude(x => x.Member)
                .ThenInclude(x => x!.AiProfile)
        .Include(x => x.Steps)
            .ThenInclude(x => x.ExecutionLogs)
                .ThenInclude(x => x.Member)
        .Include(x => x.Steps)
            .ThenInclude(x => x.HandoffToMember)
                .ThenInclude(x => x!.AiProfile)
        .Where(x => x.TeamId == teamId);

    if (ticketId.HasValue)
    {
        query = query.Where(x => x.TicketId == ticketId.Value);
    }

    if (conversationId.HasValue)
    {
        query = query.Where(x => x.ConversationId == conversationId.Value);
    }

    if (projectId.HasValue)
    {
        query = query.Where(x => x.ProjectId == projectId.Value);
    }

    var workflows = await query
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync(cancellationToken);

    return Results.Ok(workflows.Select(ToAgentWorkflowResponse).ToList());
})
.Produces<List<AgentWorkflowResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.WithName("ListAgentWorkflows")
.WithTags("Workflows");

app.MapGet("/api/teams/{teamId:guid}/workflows/{workflowId:guid}", async (
    HttpContext httpContext,
    Guid teamId,
    Guid workflowId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var workflow = await dbContext.AgentWorkflowRuns
        .Include(x => x.StartedByMember)
        .Include(x => x.Steps)
            .ThenInclude(x => x.Member)
                .ThenInclude(x => x!.AiProfile)
        .Include(x => x.Steps)
            .ThenInclude(x => x.ExecutionLogs)
                .ThenInclude(x => x.Member)
        .Include(x => x.Steps)
            .ThenInclude(x => x.HandoffToMember)
                .ThenInclude(x => x!.AiProfile)
        .FirstOrDefaultAsync(x => x.Id == workflowId && x.TeamId == teamId, cancellationToken);

    return workflow is null
        ? NotFoundError("workflow was not found", "workflow_not_found")
        : Results.Ok(ToAgentWorkflowResponse(workflow));
})
.Produces<AgentWorkflowResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("GetAgentWorkflow")
.WithTags("Workflows");

app.MapPost("/api/teams/{teamId:guid}/integrations", async (
    HttpContext httpContext,
    Guid teamId,
    CreateIntegrationConnectionRequest request,
    AppDbContext dbContext,
    AuditLogService auditLogService,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
    if (!teamExists)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var name = request.Name.Trim();
    var baseUrl = request.BaseUrl.Trim();
    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(baseUrl))
    {
        return BadRequestError("name and baseUrl are required", "integration_required_fields");
    }

    if (request.ExternalSystemType == ExternalSystemType.Unknown)
    {
        return BadRequestError("externalSystemType must be specified", "invalid_external_system_type");
    }

    var existingConnection = await dbContext.IntegrationConnections
        .AnyAsync(
            connection =>
                connection.TeamId == teamId
                && connection.ExternalSystemType == request.ExternalSystemType
                && connection.Name == name,
            cancellationToken);

    if (existingConnection)
    {
        return ConflictError("integration connection already exists in the team", "integration_connection_exists");
    }

    var connection = new IntegrationConnection
    {
        TeamId = teamId,
        ExternalSystemType = request.ExternalSystemType,
        Name = name,
        BaseUrl = baseUrl,
        AuthConfig = string.IsNullOrWhiteSpace(request.AuthConfig) ? null : request.AuthConfig.Trim(),
        IsEnabled = request.IsEnabled,
    };

    dbContext.IntegrationConnections.Add(connection);
    await dbContext.SaveChangesAsync(cancellationToken);
    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    await auditLogService.WriteAsync(
        dbContext,
        httpContext,
        "integration.create",
        "integration_connection",
        connection.Id,
        $"Integration connection {connection.Name} created for {connection.ExternalSystemType}.",
        session?.UserId,
        teamId,
        "success",
        cancellationToken);

    return Results.Created(
        $"/api/teams/{teamId}/integrations/{connection.Id}",
        ToIntegrationConnectionResponse(connection));
})
.Produces<IntegrationConnectionResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
.WithName("CreateIntegrationConnection")
.WithTags("Integrations");

app.MapGet("/api/teams/{teamId:guid}/integrations", async (
    HttpContext httpContext,
    Guid teamId,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
    if (!teamExists)
    {
        return NotFoundError("team was not found", "team_not_found");
    }

    var connections = await dbContext.IntegrationConnections
        .Where(connection => connection.TeamId == teamId)
        .OrderBy(connection => connection.ExternalSystemType)
        .ThenBy(connection => connection.Name)
        .ToListAsync(cancellationToken);

    return Results.Ok(connections.Select(ToIntegrationConnectionResponse).ToList());
})
.Produces<List<IntegrationConnectionResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("ListIntegrationConnections")
.WithTags("Integrations");

app.MapPost("/api/teams/{teamId:guid}/integrations/{connectionId:guid}/validate", async (
    HttpContext httpContext,
    Guid teamId,
    Guid connectionId,
    AppDbContext dbContext,
    AuditLogService auditLogService,
    IEnumerable<IExternalSystemAdapter> adapters,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var connection = await LoadIntegrationConnectionAsync(dbContext, teamId, connectionId, cancellationToken);
    if (connection is null)
    {
        return NotFoundError("integration connection was not found", "integration_connection_not_found");
    }

    var adapter = ResolveAdapter(adapters, connection.ExternalSystemType);
    if (adapter is null)
    {
        return BadRequestError("this connection does not support validation", "integration_capability_not_supported");
    }

    var result = await adapter.TestConnectionAsync(adapter.BuildDescriptor(connection), cancellationToken);
    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    await auditLogService.WriteAsync(
        dbContext,
        httpContext,
        "integration.validate",
        "integration_connection",
        connection.Id,
        $"Integration {connection.Name} validation result: {result.Message}",
        session?.UserId,
        teamId,
        result.IsReachable && result.IsAuthenticated ? "success" : "degraded",
        cancellationToken);
    return Results.Ok(new IntegrationConnectionHealthResponse(
        result.IsReachable,
        result.IsAuthenticated,
        result.Message,
        result.CheckedAt,
        result.SystemVersion));
})
.Produces<IntegrationConnectionHealthResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("ValidateIntegrationConnection")
.WithTags("Integrations");

app.MapGet("/api/teams/{teamId:guid}/integrations/{connectionId:guid}/files", async (
    HttpContext httpContext,
    Guid teamId,
    Guid connectionId,
    string? folderPath,
    AppDbContext dbContext,
    IEnumerable<IFileKnowledgeProvider> providers,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var connection = await LoadIntegrationConnectionAsync(dbContext, teamId, connectionId, cancellationToken);
    if (connection is null)
    {
        return NotFoundError("integration connection was not found", "integration_connection_not_found");
    }

    var provider = ResolveAdapter(providers, connection.ExternalSystemType);
    if (provider is null)
    {
        return BadRequestError("this connection does not support file knowledge preview", "integration_capability_not_supported");
    }

    var records = await provider.ListFilesAsync(provider.BuildDescriptor(connection), folderPath, cancellationToken);
    var response = records
        .Select(record => new FileKnowledgeItemResponse(
            record.Id,
            record.Name,
            record.Path,
            record.UpdatedAt,
            record.MimeType,
            record.Size))
        .ToList();

    return Results.Ok(response);
})
.Produces<List<FileKnowledgeItemResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("PreviewIntegrationFiles")
.WithTags("Integrations");

app.MapGet("/api/teams/{teamId:guid}/integrations/{connectionId:guid}/customers", async (
    HttpContext httpContext,
    Guid teamId,
    Guid connectionId,
    AppDbContext dbContext,
    IEnumerable<ICustomerProvider> providers,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var connection = await LoadIntegrationConnectionAsync(dbContext, teamId, connectionId, cancellationToken);
    if (connection is null)
    {
        return NotFoundError("integration connection was not found", "integration_connection_not_found");
    }

    var provider = ResolveAdapter(providers, connection.ExternalSystemType);
    if (provider is null)
    {
        return BadRequestError("this connection does not support customer preview", "integration_capability_not_supported");
    }

    var records = await provider.ListCustomersAsync(provider.BuildDescriptor(connection), cancellationToken);
    return Results.Ok(records.Select(ToIntegrationPreviewItemResponse).ToList());
})
.Produces<List<IntegrationPreviewItemResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("PreviewIntegrationCustomers")
.WithTags("Integrations");

app.MapGet("/api/teams/{teamId:guid}/integrations/{connectionId:guid}/projects", async (
    HttpContext httpContext,
    Guid teamId,
    Guid connectionId,
    AppDbContext dbContext,
    IEnumerable<IProjectProvider> providers,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var connection = await LoadIntegrationConnectionAsync(dbContext, teamId, connectionId, cancellationToken);
    if (connection is null)
    {
        return NotFoundError("integration connection was not found", "integration_connection_not_found");
    }

    var provider = ResolveAdapter(providers, connection.ExternalSystemType);
    if (provider is null)
    {
        return BadRequestError("this connection does not support project preview", "integration_capability_not_supported");
    }

    var records = await provider.ListProjectsAsync(provider.BuildDescriptor(connection), cancellationToken);
    return Results.Ok(records.Select(ToIntegrationPreviewItemResponse).ToList());
})
.Produces<List<IntegrationPreviewItemResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("PreviewIntegrationProjects")
.WithTags("Integrations");

app.MapGet("/api/teams/{teamId:guid}/integrations/{connectionId:guid}/tickets", async (
    HttpContext httpContext,
    Guid teamId,
    Guid connectionId,
    AppDbContext dbContext,
    IEnumerable<ITicketProvider> providers,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var connection = await LoadIntegrationConnectionAsync(dbContext, teamId, connectionId, cancellationToken);
    if (connection is null)
    {
        return NotFoundError("integration connection was not found", "integration_connection_not_found");
    }

    var provider = ResolveAdapter(providers, connection.ExternalSystemType);
    if (provider is null)
    {
        return BadRequestError("this connection does not support ticket preview", "integration_capability_not_supported");
    }

    var records = await provider.ListTicketsAsync(provider.BuildDescriptor(connection), cancellationToken);
    return Results.Ok(records.Select(ToIntegrationPreviewItemResponse).ToList());
})
.Produces<List<IntegrationPreviewItemResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("PreviewIntegrationTickets")
.WithTags("Integrations");

app.MapGet("/api/teams/{teamId:guid}/integrations/{connectionId:guid}/tasks", async (
    HttpContext httpContext,
    Guid teamId,
    Guid connectionId,
    AppDbContext dbContext,
    IEnumerable<ITaskProvider> providers,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var connection = await LoadIntegrationConnectionAsync(dbContext, teamId, connectionId, cancellationToken);
    if (connection is null)
    {
        return NotFoundError("integration connection was not found", "integration_connection_not_found");
    }

    var provider = ResolveAdapter(providers, connection.ExternalSystemType);
    if (provider is null)
    {
        return BadRequestError("this connection does not support task preview", "integration_capability_not_supported");
    }

    var records = await provider.ListTasksAsync(provider.BuildDescriptor(connection), cancellationToken);
    return Results.Ok(records.Select(ToIntegrationPreviewItemResponse).ToList());
})
.Produces<List<IntegrationPreviewItemResponse>>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("PreviewIntegrationTasks")
.WithTags("Integrations");

app.MapPatch("/api/teams/{teamId:guid}/tickets/{ticketId:guid}", async (
    HttpContext httpContext,
    Guid teamId,
    Guid ticketId,
    UpdateTicketRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);

    var ticket = await dbContext.Tickets
        .Include(x => x.Customer)
        .Include(x => x.AssignedMember)
        .FirstOrDefaultAsync(x => x.Id == ticketId && x.TeamId == teamId, cancellationToken);

    if (ticket is null)
    {
        return Results.NotFound(new ApiErrorResponse("ticket was not found"));
    }

    Member? assignedMember = null;
    if (request.AssignedMemberId.HasValue)
    {
        assignedMember = await dbContext.Members
            .FirstOrDefaultAsync(member => member.Id == request.AssignedMemberId.Value && member.TeamId == teamId,
                cancellationToken);

        if (assignedMember is null)
        {
            return Results.BadRequest(new ApiErrorResponse("assignedMemberId does not belong to the team"));
        }
    }

    var previousStatus = ticket.Status;
    var previousPriority = ticket.Priority;
    var previousAssignedMemberId = ticket.AssignedMemberId;
    var previousCategory = ticket.Category;
    var previousDueAt = ticket.DueAt;

    ticket.Status = request.Status;
    ticket.Priority = request.Priority;
    ticket.AssignedMemberId = assignedMember?.Id;
    ticket.AssignedMember = assignedMember;
    ticket.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
    ticket.DueAt = request.DueAt;
    ticket.LastActivityAt = DateTimeOffset.UtcNow;

    var activities = new List<TicketActivity>();
    if (previousStatus != request.Status)
    {
        activities.Add(CreateTicketActivity(
            ticket,
            session,
            TicketActivityType.StatusChanged,
            $"状态从 {previousStatus} 变更为 {request.Status}",
            request.ActivityNote));
    }

    if (previousPriority != request.Priority)
    {
        activities.Add(CreateTicketActivity(
            ticket,
            session,
            TicketActivityType.PriorityChanged,
            $"优先级从 {previousPriority} 调整为 {request.Priority}",
            request.ActivityNote));
    }

    if (previousAssignedMemberId != assignedMember?.Id)
    {
        activities.Add(CreateTicketActivity(
            ticket,
            session,
            TicketActivityType.AssignmentChanged,
            $"负责人更新为 {assignedMember?.DisplayName ?? "未分配"}",
            request.ActivityNote));
    }

    if (!string.Equals(previousCategory, ticket.Category, StringComparison.Ordinal))
    {
        activities.Add(CreateTicketActivity(
            ticket,
            session,
            TicketActivityType.Note,
            $"工单分类更新为 {ticket.Category ?? "未设置"}",
            request.ActivityNote));
    }

    if (previousDueAt != ticket.DueAt)
    {
        activities.Add(CreateTicketActivity(
            ticket,
            session,
            TicketActivityType.Note,
            $"期望处理时间更新为 {(ticket.DueAt.HasValue ? ticket.DueAt.Value.ToString("yyyy-MM-dd HH:mm") : "未设置")}",
            request.ActivityNote));
    }

    if (!string.IsNullOrWhiteSpace(request.ActivityNote) && activities.Count == 0)
    {
        activities.Add(CreateTicketActivity(
            ticket,
            session,
            TicketActivityType.Note,
            "补充处理备注",
            request.ActivityNote));
    }

    if (activities.Count > 0)
    {
        dbContext.TicketActivities.AddRange(activities);
    }

    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Ok(ToTicketResponse(ticket));
})
.Produces<TicketResponse>(StatusCodes.Status200OK)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("UpdateTicket")
.WithTags("Tickets");

app.MapPost("/api/teams/{teamId:guid}/tickets/{ticketId:guid}/comments", async (
    HttpContext httpContext,
    Guid teamId,
    Guid ticketId,
    AddTicketCommentRequest request,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
    if (accessError is not null)
    {
        return accessError;
    }

    var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
    var ticket = await dbContext.Tickets
        .FirstOrDefaultAsync(x => x.TeamId == teamId && x.Id == ticketId, cancellationToken);

    if (ticket is null)
    {
        return NotFoundError("ticket was not found", "ticket_not_found");
    }

    var content = request.Content.Trim();
    if (string.IsNullOrWhiteSpace(content))
    {
        return BadRequestError("content is required", "ticket_comment_required");
    }

    ticket.LastActivityAt = DateTimeOffset.UtcNow;
    var activity = CreateTicketActivity(ticket, session, TicketActivityType.Comment, "新增工单评论", content);
    dbContext.TicketActivities.Add(activity);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created(
        $"/api/teams/{teamId}/tickets/{ticketId}/comments/{activity.Id}",
        ToTicketActivityResponse(activity));
})
.Produces<TicketActivityResponse>(StatusCodes.Status201Created)
.Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
.Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
.Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
.WithName("AddTicketComment")
.WithTags("Tickets");

app.MapPost("/api/chat", async (
    ChatRequest request,
    ChatbotAgentRuntime runtime,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return BadRequestError("message is required", "chat_message_required");
    }

    var sessionId = request.SessionId ?? Guid.NewGuid().ToString("N");
    var session = await runtime.GetOrCreateSessionAsync(sessionId, cancellationToken);
    var chunks = new List<string>();

    await foreach (var update in runtime.Agent.RunStreamingAsync(request.Message, session, cancellationToken: cancellationToken))
    {
        if (!string.IsNullOrWhiteSpace(update.Text))
        {
            chunks.Add(update.Text);
        }
    }

    await runtime.SaveSessionAsync(sessionId, session, cancellationToken);

    return Results.Ok(new ChatResponse(sessionId, string.Concat(chunks)));
})
.WithName("CreateChatResponse")
.WithTags("Chat");

app.MapPost("/api/chat/stream", async (
    HttpContext httpContext,
    ChatRequest request,
    ChatbotAgentRuntime runtime,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new { error = "message is required" }, cancellationToken);
        return;
    }

    var sessionId = request.SessionId ?? Guid.NewGuid().ToString("N");
    var session = await runtime.GetOrCreateSessionAsync(sessionId, cancellationToken);

    httpContext.Response.Headers.ContentType = "text/event-stream";
    httpContext.Response.Headers.CacheControl = "no-cache";
    httpContext.Response.Headers.Connection = "keep-alive";

    await WriteSseEventAsync(httpContext, "session", new { sessionId }, cancellationToken);

    try
    {
        await foreach (var update in runtime.Agent.RunStreamingAsync(request.Message, session, cancellationToken: cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(update.Text))
            {
                continue;
            }

            await WriteSseEventAsync(httpContext, "delta", new { text = update.Text }, cancellationToken);
        }

        await runtime.SaveSessionAsync(sessionId, session, cancellationToken);
        await WriteSseEventAsync(httpContext, "completed", new { sessionId }, cancellationToken);
    }
    catch (Exception ex)
    {
        await WriteSseEventAsync(httpContext, "error", new { message = ex.Message }, cancellationToken);
    }
})
.WithName("CreateChatStream")
.WithTags("Chat");

app.MapPost("/api/chat/text-stream", async (
    HttpContext httpContext,
    AiSdkChatRequest request,
    ChatbotAgentRuntime runtime,
    CancellationToken cancellationToken) =>
{
    var message = GetLatestUserText(request.Messages);
    if (string.IsNullOrWhiteSpace(message))
    {
        return BadRequestError("A user text message is required.", "chat_message_required");
    }

    var sessionId = string.IsNullOrWhiteSpace(request.Id)
        ? Guid.NewGuid().ToString("N")
        : request.Id;

    var session = await runtime.GetOrCreateSessionAsync(sessionId, cancellationToken);

    httpContext.Response.ContentType = "text/plain; charset=utf-8";
    httpContext.Response.Headers.CacheControl = "no-cache";

    try
    {
        await foreach (var update in runtime.Agent.RunStreamingAsync(message, session, cancellationToken: cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(update.Text))
            {
                continue;
            }

            await httpContext.Response.WriteAsync(update.Text, cancellationToken);
            await httpContext.Response.Body.FlushAsync(cancellationToken);
        }

        await runtime.SaveSessionAsync(sessionId, session, cancellationToken);
        return Results.Empty;
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("CreateChatTextStream")
.WithTags("Chat");

app.MapAGUI("/agui", app.Services.GetRequiredService<ChatbotAgentRuntime>().Agent)
    .WithName("RunAguiAgent")
    .WithTags("Chat");

app.Run();

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

static UserResponse ToUserResponse(User user) =>
    new(user.Id, user.Email, user.DisplayName, user.CompanyName);

static AuditLogResponse ToAuditLogResponse(AuditLog log) =>
    new(
        log.Id,
        log.TeamId,
        log.UserId,
        log.User?.DisplayName,
        log.ActionType,
        log.EntityType,
        log.EntityId,
        log.Summary,
        log.Result,
        log.IpAddress,
        log.CreatedAt);

static UserSessionResponse ToUserSessionResponse(UserSession session, Guid currentSessionId) =>
    new(
        session.Id,
        session.CreatedAt,
        session.LastSeenAt,
        session.ExpiresAt,
        session.RevokedAt,
        session.RevokedReason,
        session.UserAgent,
        session.IpAddress,
        session.Id == currentSessionId);

static InvitationResponse ToInvitationResponse(TeamInvitation invitation) =>
    new(
        invitation.Id,
        invitation.TeamId,
        invitation.Team?.Name ?? string.Empty,
        invitation.Email,
        invitation.Role,
        invitation.Title,
        invitation.Status,
        invitation.InvitedByUser?.DisplayName ?? string.Empty,
        invitation.ExpiresAt,
        invitation.CreatedAt,
        invitation.RespondedAt);

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
                && x.RevokedAt == null
                && x.ExpiresAt > DateTimeOffset.UtcNow,
            cancellationToken);

    if (session is null)
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
        .Where(x => x.RevokedAt == null && x.ExpiresAt <= now)
        .ToListAsync(cancellationToken);

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
        .Where(x => x.Status == InvitationStatus.Pending && x.ExpiresAt <= now)
        .ToListAsync(cancellationToken);

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

static List<AiMemberTemplateResponse> GetDefaultAiMemberTemplates() =>
[
    new(
        "front-desk",
        "前台接待 AI",
        "前台接待 AI",
        "客户接待专员",
        "负责首次接待客户、确认来意、收集联系方式，并把明确需求整理成后续动作。",
        "Front Desk AI",
        "只能处理接待、摘要整理和基础分流，不能直接承诺报价或交付日期。",
        "你是团队的前台接待 AI，回答要清晰、礼貌、结构化，先收集完整信息再推进下一步。",
        "knowledge.search, conversation.summary",
        "接待客户,整理需求,推荐下一步,触发工单建议",
        "project-docs,faqs",
        false),
    new(
        "ticket-coordinator",
        "工单协调 AI",
        "工单协调 AI",
        "工单协调专员",
        "负责判断优先级、推荐负责人、推动工单进入正确状态。",
        "Ticket Ops AI",
        "可以更新工单状态和建议负责人，但不能关闭高优先级工单。",
        "你是团队的工单协调 AI，要优先保证工单信息完整、负责人明确、状态准确。",
        "ticket.list,ticket.update,team.member.lookup",
        "工单分类,优先级评估,推荐负责人,更新状态",
        "ticket-rules,team-members",
        true),
    new(
        "project-assistant",
        "项目助理 AI",
        "项目助理 AI",
        "项目助理",
        "负责整理项目上下文、汇总会话和工单，帮助老板快速了解项目状态。",
        "Project Assistant AI",
        "只能总结和建议，不直接修改客户信息或对外发送承诺。",
        "你是项目助理 AI，擅长总结、提炼风险、生成下一步建议。",
        "project.list,ticket.list,conversation.list",
        "总结进展,输出风险,整理待办,生成日报",
        "project-docs,tickets,conversations",
        false),
];

static List<WorkflowTemplateResponse> GetDefaultWorkflowTemplates() =>
[
    new(
        "customer-intake",
        "conversation",
        "客户接待链",
        "围绕当前会话整理客户意图、判断是否需要建单，并输出后续跟进建议。",
        "适合用于官网咨询、首次接待和线索筛选。"),
    new(
        "ticket-triage",
        "ticket",
        "工单分诊链",
        "围绕当前工单完成优先级判断、负责人建议、推进动作和风险说明。",
        "适合用于新工单分派、处理中工单复盘和升级判断。"),
    new(
        "project-followup",
        "project",
        "项目跟进链",
        "围绕当前项目输出现状摘要、风险提醒、依赖项和下一步建议。",
        "适合用于老板查看项目进展、周报前整理和风险对齐。"),
];

static MemberResponse ToMemberResponse(Member member) =>
    new(
        member.Id,
        member.TeamId,
        member.MemberType,
        member.Role,
        member.Status,
        member.DisplayName,
        member.Title,
        member.AiProfile is null
            ? null
            : new AiMemberProfileResponse(
                member.AiProfile.Id,
                member.AiProfile.TemplateKey,
                member.AiProfile.JobTitle,
                member.AiProfile.ResponsibilitySummary,
                member.AiProfile.PermissionBoundary,
                member.AiProfile.SystemPrompt,
                member.AiProfile.AllowedTools,
                member.AiProfile.ExecutableActions,
                member.AiProfile.KnowledgeScope,
                member.AiProfile.IsAutonomous));

static ProjectResponse ToProjectResponse(
    Project project,
    int ticketCount = 0,
    int customerCount = 0,
    IReadOnlyCollection<Guid>? participantMemberIds = null) =>
    new(
        project.Id,
        project.TeamId,
        project.Name,
        project.Description,
        project.StageLabel,
        project.Summary,
        project.RiskSummary,
        project.NextSteps,
        project.Status,
        project.LeadMemberId,
        participantMemberIds?.ToList() ?? [],
        participantMemberIds?.Count ?? 0,
        ticketCount,
        customerCount,
        project.SourceType,
        project.ExternalSystemType,
        project.ExternalId);

static ConciergeAppResponse ToConciergeAppResponse(ConciergeApp conciergeApp) =>
    new(
        conciergeApp.Id,
        conciergeApp.TeamId,
        conciergeApp.ProjectId,
        conciergeApp.Name,
        conciergeApp.Description,
        conciergeApp.ServiceScope,
        conciergeApp.WelcomeMessage,
        conciergeApp.FaqScope,
        conciergeApp.BusinessHours,
        conciergeApp.ChannelLabel,
        conciergeApp.Status,
        conciergeApp.PrimaryAiMemberId,
        conciergeApp.TicketCreationPolicy,
        conciergeApp.HumanHandoffPolicy);

static CustomerResponse ToCustomerResponse(Customer customer) =>
    new(
        customer.Id,
        customer.TeamId,
        customer.DisplayName,
        customer.Email,
        customer.PhoneNumber,
        customer.CompanyName,
        customer.SourceLabel,
        customer.Tags,
        customer.FollowUpStatus,
        customer.LastContactedAt,
        customer.ProjectId,
        customer.Notes,
        customer.Status,
        customer.SourceType,
        customer.ExternalSystemType,
        customer.ExternalId);

static TicketActivityResponse ToTicketActivityResponse(TicketActivity activity) =>
    new(
        activity.Id,
        activity.ActivityType,
        activity.Summary,
        activity.Detail,
        activity.ActorMemberId,
        activity.ActorMember?.DisplayName,
        activity.ActorUserId,
        activity.ActorUser?.DisplayName,
        activity.CreatedAt);

static TicketDetailResponse ToTicketDetailResponse(Ticket ticket) =>
    new(
        ticket.Id,
        ticket.TeamId,
        ticket.ProjectId,
        ticket.ConciergeAppId,
        ticket.CustomerId,
        ticket.Customer?.DisplayName,
        ticket.ConversationId,
        ticket.Title,
        ticket.Summary,
        ticket.Category,
        ticket.Status,
        ticket.Priority,
        ticket.DueAt,
        ticket.LastActivityAt,
        ticket.AssignedMemberId,
        ticket.AssignedMember?.DisplayName,
        ticket.Activities
            .OrderByDescending(x => x.CreatedAt)
            .Select(ToTicketActivityResponse)
            .ToList());

static TicketResponse ToTicketResponse(Ticket ticket) =>
    new(
        ticket.Id,
        ticket.TeamId,
        ticket.ProjectId,
        ticket.ConciergeAppId,
        ticket.CustomerId,
        ticket.Customer?.DisplayName,
        ticket.ConversationId,
        ticket.Title,
        ticket.Summary,
        ticket.Category,
        ticket.Status,
        ticket.Priority,
        ticket.DueAt,
        ticket.LastActivityAt,
        ticket.AssignedMemberId,
        ticket.AssignedMember?.DisplayName,
        ticket.SourceType,
        ticket.ExternalSystemType,
        ticket.ExternalId,
        ticket.CreatedAt);

static TicketActivity CreateTicketActivity(
    Ticket ticket,
    UserSession? session,
    TicketActivityType activityType,
    string summary,
    string? detail) =>
    new()
    {
        TeamId = ticket.TeamId,
        Ticket = ticket,
        ActorUserId = session?.UserId,
        ActivityType = activityType,
        Summary = summary,
        Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim(),
    };

static AgentWorkflowResponse ToAgentWorkflowResponse(AgentWorkflowRun workflow) =>
    new(
        workflow.Id,
        workflow.TeamId,
        workflow.ProjectId,
        workflow.ConversationId,
        workflow.TicketId,
        workflow.WorkflowType,
        workflow.Goal,
        workflow.Summary,
        workflow.Status,
        workflow.RequestedByUserId,
        workflow.StartedByMemberId,
        workflow.StartedByMember?.DisplayName,
        workflow.CreatedAt,
        workflow.CompletedAt,
        workflow.Steps
            .OrderBy(step => step.Sequence)
            .Select(step => new AgentWorkflowStepResponse(
                step.Id,
                step.Sequence,
                step.MemberId,
                step.Member?.DisplayName,
                step.Member?.AiProfile?.JobTitle ?? step.Member?.Title,
                step.HandoffToMemberId,
                step.HandoffToMember?.DisplayName,
                step.ActionType,
                step.InputSummary,
                step.OutputSummary,
                step.HandoffSummary,
                step.Status,
                step.ExecutedAt,
                step.ExecutionLogs
                    .OrderBy(log => log.CreatedAt)
                    .Select(log => new AgentExecutionLogResponse(
                        log.Id,
                        log.MemberId,
                        log.Member?.DisplayName,
                        log.ToolName,
                        log.ToolCategory,
                        log.BoundarySummary,
                        log.InputSummary,
                        log.OutputSummary,
                        log.Status,
                        log.WasAllowed,
                        log.ExecutedAt))
                    .ToList()))
            .ToList());

static IntegrationConnectionResponse ToIntegrationConnectionResponse(IntegrationConnection connection) =>
    new(
        connection.Id,
        connection.TeamId,
        connection.ExternalSystemType,
        connection.Name,
        connection.BaseUrl,
        connection.IsEnabled,
        !string.IsNullOrWhiteSpace(connection.AuthConfig),
        connection.CreatedAt);

static IntegrationPreviewItemResponse ToIntegrationPreviewItemResponse(IntegrationRecordRef record) =>
    new(
        record.Id,
        record.DisplayName,
        record.Summary);

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
