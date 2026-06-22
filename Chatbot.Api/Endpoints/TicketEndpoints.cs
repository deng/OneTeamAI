partial class Program
{
    static void MapTicketEndpoints(WebApplication app)
    {
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
                .OrderByDescending(ticket => ticket.CreatedAtMs)
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
            var previousResolutionSummary = ticket.ResolutionSummary;

            ticket.Status = request.Status;
            ticket.Priority = request.Priority;
            ticket.AssignedMemberId = assignedMember?.Id;
            ticket.AssignedMember = assignedMember;
            ticket.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
            ticket.DueAt = request.DueAt;
            ticket.ResolutionSummary = string.IsNullOrWhiteSpace(request.ResolutionSummary)
                ? null
                : request.ResolutionSummary.Trim();
            ticket.ResolvedAt = request.Status is TicketStatus.Completed or TicketStatus.Closed
                ? ticket.ResolvedAt ?? DateTimeOffset.UtcNow
                : null;
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

            if (!string.Equals(previousResolutionSummary, ticket.ResolutionSummary, StringComparison.Ordinal))
            {
                activities.Add(CreateTicketActivity(
                    ticket,
                    session,
                    TicketActivityType.Note,
                    $"解决结果更新为 {ticket.ResolutionSummary ?? "未填写"}",
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
    }
}
