partial class Program
{
    static void MapTeamEndpoints(WebApplication app)
    {
        // ── Team CRUD ──
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

        // ── Audit ──
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

        // ── AI Member Templates ──
        app.MapGet("/api/ai-member-templates", async (
            HttpContext httpContext,
            AppDbContext dbContext,
            Guid? teamId,
            bool? includeDisabled,
            CancellationToken cancellationToken) =>
        {
            if (teamId.HasValue)
            {
                var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId.Value, cancellationToken);
                if (accessError is not null)
                {
                    return accessError;
                }

                var teamExists = await dbContext.Teams.AnyAsync(team => team.Id == teamId.Value, cancellationToken);
                if (!teamExists)
                {
                    return NotFoundError("team was not found", "team_not_found");
                }
            }
            else if (await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken) is null)
            {
                return UnauthorizedError();
            }

            var templatesQuery = dbContext.AiMemberTemplates
                .AsNoTracking()
                .Where(template => teamId.HasValue
                    ? template.TeamId == null || template.TeamId == teamId.Value
                    : template.TeamId == null);

            if (includeDisabled != true)
            {
                templatesQuery = templatesQuery.Where(template => template.IsEnabled);
            }

            var templates = await templatesQuery
                .OrderBy(template => template.TeamId == null ? 0 : 1)
                .ThenBy(template => template.SortOrder)
                .ThenBy(template => template.Label)
                .ToListAsync(cancellationToken);

            return Results.Ok(templates.Select(ToAiMemberTemplateResponse).ToList());
        })
        .Produces<List<AiMemberTemplateResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("ListAiMemberTemplates")
        .WithTags("Members");

        app.MapPost("/api/teams/{teamId:guid}/ai-member-templates", async (
            HttpContext httpContext,
            Guid teamId,
            CreateAiMemberTemplateRequest request,
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

            var label = request.Label.Trim();
            var displayName = request.DisplayName.Trim();
            var jobTitle = request.JobTitle.Trim();
            var responsibilitySummary = request.ResponsibilitySummary.Trim();

            if (string.IsNullOrWhiteSpace(label) ||
                string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(jobTitle) ||
                string.IsNullOrWhiteSpace(responsibilitySummary))
            {
                return BadRequestError(
                    "label, displayName, jobTitle and responsibilitySummary are required",
                    "ai_member_template_required_fields");
            }

            string key;
            if (string.IsNullOrWhiteSpace(request.Key))
            {
                key = await GenerateAiMemberTemplateKeyAsync(dbContext, label, cancellationToken);
            }
            else
            {
                key = NormalizeAiMemberTemplateKey(request.Key);
                if (string.IsNullOrWhiteSpace(key))
                {
                    return BadRequestError("template key format is invalid", "invalid_ai_member_template_key");
                }

                var keyExists = await dbContext.AiMemberTemplates.AnyAsync(template => template.Key == key, cancellationToken);
                if (keyExists)
                {
                    return ConflictError("template key already exists", "ai_member_template_key_conflict");
                }
            }

            var maxSortOrder = await dbContext.AiMemberTemplates
                .Where(template => template.TeamId == teamId)
                .Select(template => (int?)template.SortOrder)
                .MaxAsync(cancellationToken);

            var template = new AiMemberTemplate
            {
                TeamId = teamId,
                Key = key,
                Label = label,
                DisplayName = displayName,
                JobTitle = jobTitle,
                ResponsibilitySummary = responsibilitySummary,
                Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
                PermissionBoundary = string.IsNullOrWhiteSpace(request.PermissionBoundary) ? null : request.PermissionBoundary.Trim(),
                SystemPrompt = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : request.SystemPrompt.Trim(),
                AllowedTools = string.IsNullOrWhiteSpace(request.AllowedTools) ? null : request.AllowedTools.Trim(),
                ExecutableActions = string.IsNullOrWhiteSpace(request.ExecutableActions) ? null : request.ExecutableActions.Trim(),
                KnowledgeScope = string.IsNullOrWhiteSpace(request.KnowledgeScope) ? null : request.KnowledgeScope.Trim(),
                IsAutonomous = request.IsAutonomous,
                IsBuiltIn = false,
                IsEnabled = true,
                SortOrder = request.SortOrder ?? ((maxSortOrder ?? 0) + 100)
            };

            dbContext.AiMemberTemplates.Add(template);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created(
                $"/api/teams/{teamId}/ai-member-templates/{template.Id}",
                ToAiMemberTemplateResponse(template));
        })
        .Produces<AiMemberTemplateResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .WithName("CreateAiMemberTemplate")
        .WithTags("Members");

        app.MapPut("/api/teams/{teamId:guid}/ai-member-templates/{templateId:guid}", async (
            HttpContext httpContext,
            Guid teamId,
            Guid templateId,
            UpdateAiMemberTemplateRequest request,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var template = await dbContext.AiMemberTemplates
                .FirstOrDefaultAsync(x => x.Id == templateId && x.TeamId == teamId, cancellationToken);
            if (template is null)
            {
                return NotFoundError("template was not found", "ai_member_template_not_found");
            }

            if (template.IsBuiltIn)
            {
                return ConflictError("built-in templates cannot be modified", "ai_member_template_builtin_locked");
            }

            var label = request.Label.Trim();
            var displayName = request.DisplayName.Trim();
            var jobTitle = request.JobTitle.Trim();
            var responsibilitySummary = request.ResponsibilitySummary.Trim();

            if (string.IsNullOrWhiteSpace(label) ||
                string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(jobTitle) ||
                string.IsNullOrWhiteSpace(responsibilitySummary))
            {
                return BadRequestError(
                    "label, displayName, jobTitle and responsibilitySummary are required",
                    "ai_member_template_required_fields");
            }

            template.Label = label;
            template.DisplayName = displayName;
            template.JobTitle = jobTitle;
            template.ResponsibilitySummary = responsibilitySummary;
            template.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();
            template.PermissionBoundary = string.IsNullOrWhiteSpace(request.PermissionBoundary) ? null : request.PermissionBoundary.Trim();
            template.SystemPrompt = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : request.SystemPrompt.Trim();
            template.AllowedTools = string.IsNullOrWhiteSpace(request.AllowedTools) ? null : request.AllowedTools.Trim();
            template.ExecutableActions = string.IsNullOrWhiteSpace(request.ExecutableActions) ? null : request.ExecutableActions.Trim();
            template.KnowledgeScope = string.IsNullOrWhiteSpace(request.KnowledgeScope) ? null : request.KnowledgeScope.Trim();
            template.IsAutonomous = request.IsAutonomous;
            template.IsEnabled = request.IsEnabled;
            if (request.SortOrder.HasValue)
            {
                template.SortOrder = request.SortOrder.Value;
            }

            template.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToAiMemberTemplateResponse(template));
        })
        .Produces<AiMemberTemplateResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .WithName("UpdateAiMemberTemplate")
        .WithTags("Members");

        app.MapDelete("/api/teams/{teamId:guid}/ai-member-templates/{templateId:guid}", async (
            HttpContext httpContext,
            Guid teamId,
            Guid templateId,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var template = await dbContext.AiMemberTemplates
                .FirstOrDefaultAsync(x => x.Id == templateId && x.TeamId == teamId, cancellationToken);
            if (template is null)
            {
                return NotFoundError("template was not found", "ai_member_template_not_found");
            }

            if (template.IsBuiltIn)
            {
                return ConflictError("built-in templates cannot be disabled", "ai_member_template_builtin_locked");
            }

            template.IsEnabled = false;
            template.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToAiMemberTemplateResponse(template));
        })
        .Produces<AiMemberTemplateResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .WithName("DisableAiMemberTemplate")
        .WithTags("Members");

        // ── Workflow Templates ──
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

        // ── Invitations ──
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
                        && x.Status == InvitationStatus.Pending,
                    cancellationToken);

            if (pendingInvitation is not null && pendingInvitation.ExpiresAt > DateTimeOffset.UtcNow)
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
    }
}
