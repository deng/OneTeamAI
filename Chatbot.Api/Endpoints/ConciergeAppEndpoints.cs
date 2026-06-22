partial class Program
{
    static void MapConciergeAppEndpoints(WebApplication app)
    {
        app.MapPost("/api/teams/{teamId:guid}/concierge-apps", async (
            HttpContext httpContext,
            Guid teamId,
            CreateConciergeAppRequest request,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, teamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var project = await dbContext.Projects
                .FirstOrDefaultAsync(x => x.Id == request.ProjectId && x.TeamId == teamId, cancellationToken);

            if (project is null)
            {
                return Results.BadRequest(new ApiErrorResponse("projectId does not belong to the team"));
            }

            Member? primaryAiMember = null;
            if (request.PrimaryAiMemberId.HasValue)
            {
                primaryAiMember = await dbContext.Members
                    .FirstOrDefaultAsync(
                        member => member.Id == request.PrimaryAiMemberId.Value
                            && member.TeamId == teamId
                            && member.MemberType == MemberType.Ai,
                        cancellationToken);

                if (primaryAiMember is null)
                {
                    return Results.BadRequest(new ApiErrorResponse("primaryAiMemberId must be an AI member in the team"));
                }
            }

            var name = request.Name.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return Results.BadRequest(new ApiErrorResponse("name is required"));
            }

            var conciergeApp = new ConciergeApp
            {
                TeamId = teamId,
                ProjectId = project.Id,
                Name = name,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                ServiceScope = string.IsNullOrWhiteSpace(request.ServiceScope) ? null : request.ServiceScope.Trim(),
                WelcomeMessage = string.IsNullOrWhiteSpace(request.WelcomeMessage) ? null : request.WelcomeMessage.Trim(),
                FaqScope = string.IsNullOrWhiteSpace(request.FaqScope) ? null : request.FaqScope.Trim(),
                BusinessHours = string.IsNullOrWhiteSpace(request.BusinessHours) ? null : request.BusinessHours.Trim(),
                ChannelLabel = string.IsNullOrWhiteSpace(request.ChannelLabel) ? null : request.ChannelLabel.Trim(),
                IntakeGuidance = string.IsNullOrWhiteSpace(request.IntakeGuidance) ? null : request.IntakeGuidance.Trim(),
                SuggestedPrompts = string.IsNullOrWhiteSpace(request.SuggestedPrompts) ? null : request.SuggestedPrompts.Trim(),
                RequireEmail = request.RequireEmail,
                RequirePhoneNumber = request.RequirePhoneNumber,
                Status = ConciergeAppStatus.Draft,
                PrimaryAiMemberId = primaryAiMember?.Id,
                TicketCreationPolicy = string.IsNullOrWhiteSpace(request.TicketCreationPolicy) ? null : request.TicketCreationPolicy.Trim(),
                HumanHandoffPolicy = string.IsNullOrWhiteSpace(request.HumanHandoffPolicy) ? null : request.HumanHandoffPolicy.Trim(),
            };

            dbContext.ConciergeApps.Add(conciergeApp);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created(
                $"/api/teams/{teamId}/concierge-apps/{conciergeApp.Id}",
                ToConciergeAppResponse(conciergeApp));
        })
        .Produces<ConciergeAppResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .WithName("CreateConciergeApp")
        .WithTags("ConciergeApps");

        app.MapGet("/api/teams/{teamId:guid}/concierge-apps", async (
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
                return Results.NotFound(new ApiErrorResponse("team was not found"));
            }

            var apps = await dbContext.ConciergeApps
                .Where(appEntity => appEntity.TeamId == teamId)
                .OrderBy(appEntity => appEntity.CreatedAtMs)
                .ToListAsync(cancellationToken);

            return Results.Ok(apps.Select(ToConciergeAppResponse).ToList());
        })
        .Produces<List<ConciergeAppResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("ListConciergeApps")
        .WithTags("ConciergeApps");

        app.MapPatch("/api/teams/{teamId:guid}/concierge-apps/{conciergeAppId:guid}", async (
            HttpContext httpContext,
            Guid teamId,
            Guid conciergeAppId,
            UpdateConciergeAppRequest request,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var conciergeApp = await dbContext.ConciergeApps
                .FirstOrDefaultAsync(x => x.Id == conciergeAppId && x.TeamId == teamId, cancellationToken);

            if (conciergeApp is null)
            {
                return NotFoundError("concierge app was not found", "concierge_app_not_found");
            }

            var primaryAiMemberId = request.PrimaryAiMemberId;
            if (primaryAiMemberId.HasValue)
            {
                var primaryAiMemberExists = await dbContext.Members
                    .AnyAsync(
                        member => member.Id == primaryAiMemberId.Value
                            && member.TeamId == teamId
                            && member.MemberType == MemberType.Ai,
                        cancellationToken);

                if (!primaryAiMemberExists)
                {
                    return BadRequestError("primaryAiMemberId must be an AI member in the team", "invalid_primary_ai_member");
                }
            }

            var name = request.Name.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequestError("name is required", "concierge_name_required");
            }

            conciergeApp.Name = name;
            conciergeApp.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            conciergeApp.ServiceScope = string.IsNullOrWhiteSpace(request.ServiceScope) ? null : request.ServiceScope.Trim();
            conciergeApp.WelcomeMessage = string.IsNullOrWhiteSpace(request.WelcomeMessage) ? null : request.WelcomeMessage.Trim();
            conciergeApp.FaqScope = string.IsNullOrWhiteSpace(request.FaqScope) ? null : request.FaqScope.Trim();
            conciergeApp.BusinessHours = string.IsNullOrWhiteSpace(request.BusinessHours) ? null : request.BusinessHours.Trim();
            conciergeApp.ChannelLabel = string.IsNullOrWhiteSpace(request.ChannelLabel) ? null : request.ChannelLabel.Trim();
            conciergeApp.IntakeGuidance = string.IsNullOrWhiteSpace(request.IntakeGuidance) ? null : request.IntakeGuidance.Trim();
            conciergeApp.SuggestedPrompts = string.IsNullOrWhiteSpace(request.SuggestedPrompts) ? null : request.SuggestedPrompts.Trim();
            conciergeApp.RequireEmail = request.RequireEmail;
            conciergeApp.RequirePhoneNumber = request.RequirePhoneNumber;
            conciergeApp.Status = request.Status;
            conciergeApp.PrimaryAiMemberId = primaryAiMemberId;
            conciergeApp.TicketCreationPolicy = string.IsNullOrWhiteSpace(request.TicketCreationPolicy) ? null : request.TicketCreationPolicy.Trim();
            conciergeApp.HumanHandoffPolicy = string.IsNullOrWhiteSpace(request.HumanHandoffPolicy) ? null : request.HumanHandoffPolicy.Trim();
            conciergeApp.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok(ToConciergeAppResponse(conciergeApp));
        })
        .Produces<ConciergeAppResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("UpdateConciergeApp")
        .WithTags("ConciergeApps");

        // ── Public Concierge ──
        app.MapGet("/api/public/concierge-apps/{conciergeAppId:guid}", async (
            Guid conciergeAppId,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var conciergeApp = await dbContext.ConciergeApps
                .Include(x => x.Team)
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == conciergeAppId, cancellationToken);

            if (conciergeApp is null || conciergeApp.Status == ConciergeAppStatus.Archived)
            {
                return NotFoundError("concierge app was not found", "concierge_app_not_found");
            }

            return Results.Ok(new PublicConciergeAppResponse(
                conciergeApp.Id,
                conciergeApp.Name,
                conciergeApp.Description,
                conciergeApp.ServiceScope,
                conciergeApp.WelcomeMessage,
                conciergeApp.FaqScope,
                conciergeApp.BusinessHours,
                conciergeApp.ChannelLabel,
                conciergeApp.IntakeGuidance,
                conciergeApp.SuggestedPrompts,
                conciergeApp.RequireEmail,
                conciergeApp.RequirePhoneNumber,
                conciergeApp.Status,
                conciergeApp.Team?.BrandName,
                conciergeApp.Project?.Name));
        })
        .Produces<PublicConciergeAppResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("GetPublicConciergeApp")
        .WithTags("PublicConcierge");

        app.MapPost("/api/public/concierge-apps/{conciergeAppId:guid}/intake", async (
            Guid conciergeAppId,
            PublicConciergeIntakeRequest request,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var conciergeApp = await dbContext.ConciergeApps
                .Include(x => x.Team)
                .FirstOrDefaultAsync(x => x.Id == conciergeAppId, cancellationToken);

            if (conciergeApp is null || conciergeApp.Status == ConciergeAppStatus.Archived)
            {
                return NotFoundError("concierge app was not found", "concierge_app_not_found");
            }

            if (conciergeApp.Status == ConciergeAppStatus.Inactive)
            {
                return BadRequestError("concierge app is inactive", "concierge_app_inactive");
            }

            var displayName = request.DisplayName.Trim();
            var message = request.Message.Trim();
            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            var companyName = string.IsNullOrWhiteSpace(request.CompanyName) ? null : request.CompanyName.Trim();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                return BadRequestError("displayName is required", "display_name_required");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequestError("message is required", "message_required");
            }

            if (email is not null && !IsValidEmail(email))
            {
                return BadRequestError("email format is invalid", "invalid_email");
            }

            if (conciergeApp.RequireEmail && email is null)
            {
                return BadRequestError("email is required", "email_required");
            }

            if (conciergeApp.RequirePhoneNumber && phoneNumber is null)
            {
                return BadRequestError("phone number is required", "phone_number_required");
            }

            Customer? customer = null;
            if (email is not null)
            {
                customer = await dbContext.Customers
                    .FirstOrDefaultAsync(x => x.TeamId == conciergeApp.TeamId && x.Email == email, cancellationToken);
            }

            if (customer is null)
            {
                customer = new Customer
                {
                    TeamId = conciergeApp.TeamId,
                    DisplayName = displayName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    CompanyName = companyName,
                    SourceLabel = conciergeApp.ChannelLabel,
                    Status = email is null ? CustomerStatus.Anonymous : CustomerStatus.Active,
                };
                dbContext.Customers.Add(customer);
            }
            else
            {
                customer.DisplayName = displayName;
                customer.PhoneNumber = phoneNumber ?? customer.PhoneNumber;
                customer.CompanyName = companyName ?? customer.CompanyName;
                customer.SourceLabel ??= conciergeApp.ChannelLabel;
                customer.UpdatedAt = DateTimeOffset.UtcNow;
            }

            var conversation = new Conversation
            {
                TeamId = conciergeApp.TeamId,
                ConciergeAppId = conciergeApp.Id,
                Customer = customer,
                Status = ConversationStatus.Open,
                Messages =
                [
                    new ConversationMessage
                    {
                        ParticipantType = ConversationParticipantType.Customer,
                        Content = message,
                        SenderName = displayName,
                    }
                ]
            };

            dbContext.Conversations.Add(conversation);

            Ticket? ticket = null;
            if (request.AutoCreateTicket)
            {
                ticket = new Ticket
                {
                    TeamId = conciergeApp.TeamId,
                    ProjectId = conciergeApp.ProjectId,
                    ConciergeAppId = conciergeApp.Id,
                    Customer = customer,
                    Conversation = conversation,
                    Title = BuildTicketTitle(message),
                    Summary = message,
                    Status = TicketStatus.Pending,
                    Priority = request.AutoTicketPriority,
                };
                dbContext.Tickets.Add(ticket);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created(
                $"/api/public/concierge-apps/{conciergeAppId}/intake/{conversation.Id}",
                new PublicConciergeIntakeResponse(
                    customer.Id,
                    conversation.Id,
                    ticket?.Id,
                    "已收到你的需求，我们会尽快跟进。"));
        })
        .Produces<PublicConciergeIntakeResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("CreatePublicConciergeIntake")
        .WithTags("PublicConcierge");
    }
}
