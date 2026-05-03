partial class Program
{
    static void MapIntegrationPreviewEndpoints(WebApplication app)
    {
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
    }
}
