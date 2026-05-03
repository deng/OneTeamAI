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
                return BadRequestError("name is required", "team_name_required");
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
    }
}
