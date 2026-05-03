partial class Program
{
    static void MapTeamTemplateEndpoints(WebApplication app)
    {
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
    }
}
