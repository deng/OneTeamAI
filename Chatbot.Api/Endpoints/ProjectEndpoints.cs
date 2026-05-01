partial class Program
{
    static void MapProjectEndpoints(WebApplication app)
    {
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
    }
}
