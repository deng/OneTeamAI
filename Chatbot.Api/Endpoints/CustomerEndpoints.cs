partial class Program
{
    static void MapCustomerEndpoints(WebApplication app)
    {
        app.MapPost("/api/teams/{teamId:guid}/customers", async (
            HttpContext httpContext,
            Guid teamId,
            CreateCustomerRequest request,
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
                return Results.NotFound(new ApiErrorResponse("team was not found"));
            }

            var displayName = request.DisplayName.Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return Results.BadRequest(new ApiErrorResponse("displayName is required"));
            }

            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            if (email is not null && !IsValidEmail(email))
            {
                return BadRequestError("email format is invalid", "invalid_email");
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var existingCustomer = await dbContext.Customers
                    .FirstOrDefaultAsync(customer => customer.TeamId == teamId && customer.Email == email, cancellationToken);

                if (existingCustomer is not null)
                {
                    return ConflictError("customer email already exists in the team", "customer_email_exists");
                }
            }

            if (request.ProjectId.HasValue)
            {
                var projectExists = await dbContext.Projects
                    .AnyAsync(project => project.Id == request.ProjectId.Value && project.TeamId == teamId, cancellationToken);

                if (!projectExists)
                {
                    return BadRequestError("projectId does not belong to the team", "invalid_project_id");
                }
            }

            var customer = new Customer
            {
                TeamId = teamId,
                DisplayName = displayName,
                Email = email,
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                CompanyName = string.IsNullOrWhiteSpace(request.CompanyName) ? null : request.CompanyName.Trim(),
                SourceLabel = string.IsNullOrWhiteSpace(request.SourceLabel) ? null : request.SourceLabel.Trim(),
                Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags.Trim(),
                FollowUpStatus = request.FollowUpStatus,
                LastContactedAt = request.LastContactedAt,
                ProjectId = request.ProjectId,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                Status = email is null ? CustomerStatus.Anonymous : CustomerStatus.Active,
            };

            dbContext.Customers.Add(customer);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/teams/{teamId}/customers/{customer.Id}", ToCustomerResponse(customer));
        })
        .Produces<CustomerResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .WithName("CreateCustomer")
        .WithTags("Customers");

        app.MapPatch("/api/teams/{teamId:guid}/customers/{customerId:guid}", async (
            HttpContext httpContext,
            Guid teamId,
            Guid customerId,
            UpdateCustomerRequest request,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var customer = await dbContext.Customers
                .FirstOrDefaultAsync(x => x.Id == customerId && x.TeamId == teamId, cancellationToken);

            if (customer is null)
            {
                return NotFoundError("customer was not found", "customer_not_found");
            }

            var displayName = request.DisplayName.Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return BadRequestError("displayName is required", "display_name_required");
            }

            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            if (email is not null && !IsValidEmail(email))
            {
                return BadRequestError("email format is invalid", "invalid_email");
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailConflict = await dbContext.Customers
                    .AnyAsync(x => x.TeamId == teamId && x.Email == email && x.Id != customerId, cancellationToken);

                if (emailConflict)
                {
                    return ConflictError("customer email already exists in the team", "customer_email_exists");
                }
            }

            if (request.ProjectId.HasValue)
            {
                var projectExists = await dbContext.Projects
                    .AnyAsync(project => project.Id == request.ProjectId.Value && project.TeamId == teamId, cancellationToken);

                if (!projectExists)
                {
                    return BadRequestError("projectId does not belong to the team", "invalid_project_id");
                }
            }

            customer.DisplayName = displayName;
            customer.Email = email;
            customer.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            customer.CompanyName = string.IsNullOrWhiteSpace(request.CompanyName) ? null : request.CompanyName.Trim();
            customer.SourceLabel = string.IsNullOrWhiteSpace(request.SourceLabel) ? null : request.SourceLabel.Trim();
            customer.Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags.Trim();
            customer.FollowUpStatus = request.FollowUpStatus;
            customer.LastContactedAt = request.LastContactedAt;
            customer.ProjectId = request.ProjectId;
            customer.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            customer.Status = request.Status;
            customer.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok(ToCustomerResponse(customer));
        })
        .Produces<CustomerResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .WithName("UpdateCustomer")
        .WithTags("Customers");

        app.MapGet("/api/teams/{teamId:guid}/customers", async (
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

            var customers = await dbContext.Customers
                .Where(customer => customer.TeamId == teamId)
                .OrderBy(customer => customer.CreatedAtMs)
                .ToListAsync(cancellationToken);

            return Results.Ok(customers.Select(ToCustomerResponse).ToList());
        })
        .Produces<List<CustomerResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("ListCustomers")
        .WithTags("Customers");
    }
}
