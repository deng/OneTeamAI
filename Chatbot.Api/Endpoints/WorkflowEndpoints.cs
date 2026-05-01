partial class Program
{
    static void MapWorkflowEndpoints(WebApplication app)
    {
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
                request.Goal,
                request.TriggerMode);

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
                request.Goal,
                request.TriggerMode);

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
                request.Goal,
                request.TriggerMode);

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
    }
}
