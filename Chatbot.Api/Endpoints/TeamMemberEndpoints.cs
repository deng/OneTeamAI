partial class Program
{
    static void MapTeamMemberEndpoints(WebApplication app)
    {
        // ── Members ──
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
                return NotFoundError("team was not found", "team_not_found");
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
    }
}
