partial class Program
{
    static void MapIntegrationEndpoints(WebApplication app)
    {
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

        app.MapPost("/api/teams/{teamId:guid}/integrations/{connectionId:guid}/customers/import", async (
            HttpContext httpContext,
            Guid teamId,
            Guid connectionId,
            ImportIntegrationCustomerRequest request,
            AppDbContext dbContext,
            AuditLogService auditLogService,
            IEnumerable<ICustomerProvider> providers,
            CancellationToken cancellationToken) =>
        {
            var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var externalRecordId = request.ExternalRecordId.Trim();
            if (string.IsNullOrWhiteSpace(externalRecordId))
            {
                return BadRequestError("externalRecordId is required", "integration_customer_import_required_id");
            }

            if (request.ProjectId.HasValue)
            {
                var projectExists = await dbContext.Projects.AnyAsync(
                    project => project.Id == request.ProjectId.Value && project.TeamId == teamId,
                    cancellationToken);
                if (!projectExists)
                {
                    return BadRequestError("projectId does not belong to the team", "invalid_project_id");
                }
            }

            var connection = await LoadIntegrationConnectionAsync(dbContext, teamId, connectionId, cancellationToken);
            if (connection is null)
            {
                return NotFoundError("integration connection was not found", "integration_connection_not_found");
            }

            var provider = ResolveAdapter(providers, connection.ExternalSystemType);
            if (provider is null)
            {
                return BadRequestError("this connection does not support customer import", "integration_capability_not_supported");
            }

            var records = await provider.ListCustomersAsync(provider.BuildDescriptor(connection), cancellationToken);
            var record = records.FirstOrDefault(item => string.Equals(item.Id, externalRecordId, StringComparison.Ordinal));
            if (record is null)
            {
                return NotFoundError("integration customer record was not found", "integration_customer_not_found");
            }

            var customer = await dbContext.Customers.FirstOrDefaultAsync(
                item =>
                    item.TeamId == teamId
                    && item.ExternalSystemType == connection.ExternalSystemType
                    && item.ExternalId == externalRecordId,
                cancellationToken);

            var isNew = customer is null;
            if (!isNew && !request.ForceUpdate)
            {
                var conflictSession = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
                await auditLogService.WriteAsync(
                    dbContext,
                    httpContext,
                    "integration.import_customer",
                    "customer",
                    customer!.Id,
                    $"Customer {customer.DisplayName} import conflicted from {connection.Name}.",
                    conflictSession?.UserId,
                    teamId,
                    "conflict",
                    cancellationToken);
                return ConflictError(
                    "integration customer already imported, use forceUpdate to sync",
                    "integration_customer_already_imported");
            }

            if (customer is null)
            {
                customer = new Customer
                {
                    TeamId = teamId,
                    SourceType = RecordSourceType.External,
                    ExternalSystemType = connection.ExternalSystemType,
                    ExternalId = externalRecordId,
                    Status = CustomerStatus.Active,
                };
                dbContext.Customers.Add(customer);
            }

            customer.DisplayName = record.DisplayName.Trim();
            customer.SourceLabel = connection.Name;
            customer.CompanyName = string.IsNullOrWhiteSpace(record.Summary) ? customer.CompanyName : record.Summary.Trim();
            customer.ProjectId = request.ProjectId ?? customer.ProjectId;
            customer.Tags = string.IsNullOrWhiteSpace(request.Tags) ? customer.Tags : request.Tags.Trim();
            customer.FollowUpStatus = CustomerFollowUpStatus.New;
            customer.LastContactedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            await auditLogService.WriteAsync(
                dbContext,
                httpContext,
                "integration.import_customer",
                "customer",
                customer.Id,
                $"Customer {customer.DisplayName} {(isNew ? "imported" : "synced")} from {connection.Name}.",
                session?.UserId,
                teamId,
                "success",
                cancellationToken);

            return isNew
                ? Results.Created($"/api/teams/{teamId}/customers/{customer.Id}", ToCustomerResponse(customer))
                : Results.Ok(ToCustomerResponse(customer));
        })
        .Produces<CustomerResponse>(StatusCodes.Status200OK)
        .Produces<CustomerResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .WithName("ImportIntegrationCustomer")
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

        app.MapPost("/api/teams/{teamId:guid}/integrations/{connectionId:guid}/projects/import", async (
            HttpContext httpContext,
            Guid teamId,
            Guid connectionId,
            ImportIntegrationProjectRequest request,
            AppDbContext dbContext,
            AuditLogService auditLogService,
            IEnumerable<IProjectProvider> providers,
            CancellationToken cancellationToken) =>
        {
            var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var externalRecordId = request.ExternalRecordId.Trim();
            if (string.IsNullOrWhiteSpace(externalRecordId))
            {
                return BadRequestError("externalRecordId is required", "integration_project_import_required_id");
            }

            Member? leadMember = null;
            if (request.LeadMemberId.HasValue)
            {
                leadMember = await dbContext.Members.FirstOrDefaultAsync(
                    member => member.Id == request.LeadMemberId.Value && member.TeamId == teamId,
                    cancellationToken);
                if (leadMember is null)
                {
                    return BadRequestError("leadMemberId does not belong to the team", "invalid_lead_member_id");
                }
            }

            var connection = await LoadIntegrationConnectionAsync(dbContext, teamId, connectionId, cancellationToken);
            if (connection is null)
            {
                return NotFoundError("integration connection was not found", "integration_connection_not_found");
            }

            var provider = ResolveAdapter(providers, connection.ExternalSystemType);
            if (provider is null)
            {
                return BadRequestError("this connection does not support project import", "integration_capability_not_supported");
            }

            var records = await provider.ListProjectsAsync(provider.BuildDescriptor(connection), cancellationToken);
            var record = records.FirstOrDefault(item => string.Equals(item.Id, externalRecordId, StringComparison.Ordinal));
            if (record is null)
            {
                return NotFoundError("integration project record was not found", "integration_project_not_found");
            }

            var project = await dbContext.Projects
                .Include(item => item.ProjectMembers)
                .FirstOrDefaultAsync(
                    item =>
                        item.TeamId == teamId
                        && item.ExternalSystemType == connection.ExternalSystemType
                        && item.ExternalId == externalRecordId,
                    cancellationToken);

            var isNew = project is null;
            if (!isNew && !request.ForceUpdate)
            {
                var conflictSession = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
                await auditLogService.WriteAsync(
                    dbContext,
                    httpContext,
                    "integration.import_project",
                    "project",
                    project!.Id,
                    $"Project {project.Name} import conflicted from {connection.Name}.",
                    conflictSession?.UserId,
                    teamId,
                    "conflict",
                    cancellationToken);
                return ConflictError(
                    "integration project already imported, use forceUpdate to sync",
                    "integration_project_already_imported");
            }

            if (project is null)
            {
                project = new Project
                {
                    TeamId = teamId,
                    SourceType = RecordSourceType.External,
                    ExternalSystemType = connection.ExternalSystemType,
                    ExternalId = externalRecordId,
                    Status = ProjectStatus.Active,
                };
                dbContext.Projects.Add(project);
            }

            project.Name = record.DisplayName.Trim();
            project.StageLabel = string.IsNullOrWhiteSpace(record.Summary) ? project.StageLabel : record.Summary.Trim();
            project.Summary = $"Imported from {connection.Name}";
            project.LeadMemberId = leadMember?.Id ?? project.LeadMemberId;

            await dbContext.SaveChangesAsync(cancellationToken);

            var ticketCount = await dbContext.Tickets.CountAsync(ticket => ticket.ProjectId == project.Id, cancellationToken);
            var customerCount = await dbContext.Customers.CountAsync(customer => customer.ProjectId == project.Id, cancellationToken);
            var participantMemberIds = project.ProjectMembers.Select(member => member.MemberId).ToList();

            var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            await auditLogService.WriteAsync(
                dbContext,
                httpContext,
                "integration.import_project",
                "project",
                project.Id,
                $"Project {project.Name} {(isNew ? "imported" : "synced")} from {connection.Name}.",
                session?.UserId,
                teamId,
                "success",
                cancellationToken);

            var response = ToProjectResponse(project, ticketCount, customerCount, participantMemberIds);
            return isNew
                ? Results.Created($"/api/teams/{teamId}/projects/{project.Id}", response)
                : Results.Ok(response);
        })
        .Produces<ProjectResponse>(StatusCodes.Status200OK)
        .Produces<ProjectResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .WithName("ImportIntegrationProject")
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

        app.MapPost("/api/teams/{teamId:guid}/integrations/{connectionId:guid}/tickets/import", async (
            HttpContext httpContext,
            Guid teamId,
            Guid connectionId,
            ImportIntegrationTicketRequest request,
            AppDbContext dbContext,
            AuditLogService auditLogService,
            IEnumerable<ITicketProvider> providers,
            CancellationToken cancellationToken) =>
        {
            var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var externalRecordId = request.ExternalRecordId.Trim();
            if (string.IsNullOrWhiteSpace(externalRecordId))
            {
                return BadRequestError("externalRecordId is required", "integration_ticket_import_required_id");
            }

            var project = await dbContext.Projects.FirstOrDefaultAsync(
                item => item.Id == request.ProjectId && item.TeamId == teamId,
                cancellationToken);
            if (project is null)
            {
                return BadRequestError("projectId does not belong to the team", "invalid_project_id");
            }

            Customer? customer = null;
            if (request.CustomerId.HasValue)
            {
                customer = await dbContext.Customers.FirstOrDefaultAsync(
                    item => item.Id == request.CustomerId.Value && item.TeamId == teamId,
                    cancellationToken);
                if (customer is null)
                {
                    return BadRequestError("customerId does not belong to the team", "invalid_customer_id");
                }
            }

            var connection = await LoadIntegrationConnectionAsync(dbContext, teamId, connectionId, cancellationToken);
            if (connection is null)
            {
                return NotFoundError("integration connection was not found", "integration_connection_not_found");
            }

            var provider = ResolveAdapter(providers, connection.ExternalSystemType);
            if (provider is null)
            {
                return BadRequestError("this connection does not support ticket import", "integration_capability_not_supported");
            }

            var records = await provider.ListTicketsAsync(provider.BuildDescriptor(connection), cancellationToken);
            var record = records.FirstOrDefault(item => string.Equals(item.Id, externalRecordId, StringComparison.Ordinal));
            if (record is null)
            {
                return NotFoundError("integration ticket record was not found", "integration_ticket_not_found");
            }

            var ticket = await dbContext.Tickets.FirstOrDefaultAsync(
                item =>
                    item.TeamId == teamId
                    && item.ExternalSystemType == connection.ExternalSystemType
                    && item.ExternalId == externalRecordId,
                cancellationToken);

            var isNew = ticket is null;
            if (!isNew && !request.ForceUpdate)
            {
                var conflictSession = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
                await auditLogService.WriteAsync(
                    dbContext,
                    httpContext,
                    "integration.import_ticket",
                    "ticket",
                    ticket!.Id,
                    $"Ticket {ticket.Title} import conflicted from {connection.Name}.",
                    conflictSession?.UserId,
                    teamId,
                    "conflict",
                    cancellationToken);
                return ConflictError(
                    "integration ticket already imported, use forceUpdate to sync",
                    "integration_ticket_already_imported");
            }

            if (ticket is null)
            {
                ticket = new Ticket
                {
                    TeamId = teamId,
                    ProjectId = project.Id,
                    CustomerId = customer?.Id,
                    SourceType = RecordSourceType.External,
                    ExternalSystemType = connection.ExternalSystemType,
                    ExternalId = externalRecordId,
                    Status = TicketStatus.Pending,
                    Priority = TicketPriority.Medium,
                };
                dbContext.Tickets.Add(ticket);
            }

            ticket.ProjectId = project.Id;
            ticket.CustomerId = customer?.Id ?? ticket.CustomerId;
            ticket.Title = record.DisplayName.Trim();
            ticket.Summary = string.IsNullOrWhiteSpace(record.Summary)
                ? $"Imported from {connection.Name}"
                : record.Summary.Trim();
            ticket.LastActivityAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            await auditLogService.WriteAsync(
                dbContext,
                httpContext,
                "integration.import_ticket",
                "ticket",
                ticket.Id,
                $"Ticket {ticket.Title} {(isNew ? "imported" : "synced")} from {connection.Name}.",
                session?.UserId,
                teamId,
                "success",
                cancellationToken);

            var response = ToTicketResponse(ticket);
            return isNew
                ? Results.Created($"/api/teams/{teamId}/tickets/{ticket.Id}", response)
                : Results.Ok(response);
        })
        .Produces<TicketResponse>(StatusCodes.Status200OK)
        .Produces<TicketResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .WithName("ImportIntegrationTicket")
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
    }
}
