partial class Program
{
    static void MapIntegrationConnectionEndpoints(WebApplication app)
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
    }
}
