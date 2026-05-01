partial class Program
{
    static void MapConversationEndpoints(WebApplication app)
    {
        app.MapPost("/api/concierge-apps/{conciergeAppId:guid}/conversations", async (
            HttpContext httpContext,
            Guid conciergeAppId,
            CreateConversationRequest request,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var conciergeApp = await dbContext.ConciergeApps
                .FirstOrDefaultAsync(x => x.Id == conciergeAppId, cancellationToken);

            if (conciergeApp is null)
            {
                return Results.NotFound(new ApiErrorResponse("concierge app was not found"));
            }

            var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, conciergeApp.TeamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var initialMessage = request.InitialMessage.Trim();
            if (string.IsNullOrWhiteSpace(initialMessage))
            {
                return BadRequestError("initialMessage is required", "initial_message_required");
            }

            Customer? customer = null;
            if (request.CustomerId.HasValue)
            {
                customer = await dbContext.Customers
                    .FirstOrDefaultAsync(
                        x => x.Id == request.CustomerId.Value && x.TeamId == conciergeApp.TeamId,
                        cancellationToken);

                if (customer is null)
                {
                    return Results.BadRequest(new ApiErrorResponse("customerId does not belong to the concierge app team"));
                }
            }
            else if (!string.IsNullOrWhiteSpace(request.CustomerDisplayName))
            {
                var displayName = request.CustomerDisplayName.Trim();
                var email = string.IsNullOrWhiteSpace(request.CustomerEmail) ? null : request.CustomerEmail.Trim();

                if (email is not null && !IsValidEmail(email))
                {
                    return BadRequestError("customerEmail format is invalid", "invalid_email");
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    customer = await dbContext.Customers
                        .FirstOrDefaultAsync(
                            x => x.TeamId == conciergeApp.TeamId && x.Email == email,
                            cancellationToken);
                }

                if (customer is null)
                {
                    customer = new Customer
                    {
                        TeamId = conciergeApp.TeamId,
                        DisplayName = displayName,
                        Email = email,
                        Status = email is null ? CustomerStatus.Anonymous : CustomerStatus.Active,
                    };
                    dbContext.Customers.Add(customer);
                }
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
                        SenderName = customer?.DisplayName,
                        Content = initialMessage,
                    }
                ]
            };

            dbContext.Conversations.Add(conversation);

            Ticket? ticket = null;
            if (request.AutoCreateTicket)
            {
                var title = BuildTicketTitle(initialMessage);
                ticket = new Ticket
                {
                    TeamId = conciergeApp.TeamId,
                    ProjectId = conciergeApp.ProjectId,
                    ConciergeAppId = conciergeApp.Id,
                    Customer = customer,
                    Conversation = conversation,
                    Title = title,
                    Summary = initialMessage,
                    Category = "客户咨询",
                    Status = TicketStatus.Pending,
                    Priority = request.AutoTicketPriority,
                    LastActivityAt = DateTimeOffset.UtcNow,
                };

                dbContext.Tickets.Add(ticket);
                dbContext.TicketActivities.Add(new TicketActivity
                {
                    TeamId = conciergeApp.TeamId,
                    Ticket = ticket,
                    ActivityType = TicketActivityType.Created,
                    Summary = "自动从会话创建工单",
                    Detail = initialMessage,
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created(
                $"/api/conversations/{conversation.Id}",
                new ConversationResponse(
                    conversation.Id,
                    conversation.TeamId,
                    conversation.ConciergeAppId,
                    conversation.CustomerId,
                    conversation.Status,
                    initialMessage,
                    ticket?.Id));
        })
        .Produces<ConversationResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("CreateConversation")
        .WithTags("Conversations");

        app.MapGet("/api/concierge-apps/{conciergeAppId:guid}/conversations", async (
            HttpContext httpContext,
            Guid conciergeAppId,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var conciergeApp = await dbContext.ConciergeApps
                .FirstOrDefaultAsync(x => x.Id == conciergeAppId, cancellationToken);
            if (conciergeApp is null)
            {
                return Results.NotFound(new ApiErrorResponse("concierge app was not found"));
            }

            var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, conciergeApp.TeamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var conversations = await dbContext.Conversations
                .Where(conversation => conversation.ConciergeAppId == conciergeAppId)
                .Include(conversation => conversation.Customer)
                .Include(conversation => conversation.Messages)
                .OrderByDescending(conversation => conversation.CreatedAt)
                .ToListAsync(cancellationToken);

            var result = conversations.Select(conversation => new ConversationSummaryResponse(
                conversation.Id,
                conversation.TeamId,
                conversation.ConciergeAppId,
                conversation.CustomerId,
                conversation.Customer?.DisplayName,
                conversation.Status,
                conversation.Messages.Count,
                conversation.Messages
                    .OrderByDescending(message => message.CreatedAt)
                    .Select(message => message.Content)
                    .FirstOrDefault(),
                conversation.CreatedAt)).ToList();

            return Results.Ok(result);
        })
        .Produces<List<ConversationSummaryResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("ListConversations")
        .WithTags("Conversations");

        app.MapGet("/api/conversations/{conversationId:guid}", async (
            HttpContext httpContext,
            Guid conversationId,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var conversation = await dbContext.Conversations
                .Include(x => x.Customer)
                .Include(x => x.Messages)
                .Include(x => x.Tickets)
                .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

            if (conversation is null)
            {
                return Results.NotFound(new ApiErrorResponse("conversation was not found"));
            }

            var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, conversation.TeamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            return Results.Ok(new ConversationDetailResponse(
                conversation.Id,
                conversation.TeamId,
                conversation.ConciergeAppId,
                conversation.CustomerId,
                conversation.Customer is null
                    ? null
                    : new ConversationCustomerResponse(
                        conversation.Customer.DisplayName,
                        conversation.Customer.Email),
                conversation.Status,
                conversation.Messages
                    .OrderBy(message => message.CreatedAt)
                    .Select(message => new ConversationMessageResponse(
                        message.Id,
                        message.ParticipantType,
                        message.MemberId,
                        message.SenderName,
                        message.Content,
                        message.CreatedAt))
                    .ToList(),
                conversation.Tickets.Select(ticket => ticket.Id).ToList()));
        })
        .Produces<ConversationDetailResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("GetConversation")
        .WithTags("Conversations");

        app.MapPost("/api/conversations/{conversationId:guid}/tickets", async (
            HttpContext httpContext,
            Guid conversationId,
            CreateTicketRequest request,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var conversation = await dbContext.Conversations
                .Include(x => x.ConciergeApp)
                .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

            if (conversation is null)
            {
                return Results.NotFound(new ApiErrorResponse("conversation was not found"));
            }

            var accessError = await EnsureTeamAccessAsync(httpContext, dbContext, conversation.TeamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var title = request.Title.Trim();
            var summary = request.Summary.Trim();
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(summary))
            {
                return BadRequestError("title and summary are required", "ticket_required_fields");
            }

            Member? assignedMember = null;
            if (request.AssignedMemberId.HasValue)
            {
                assignedMember = await dbContext.Members
                    .FirstOrDefaultAsync(
                        member => member.Id == request.AssignedMemberId.Value && member.TeamId == conversation.TeamId,
                        cancellationToken);

                if (assignedMember is null)
                {
                    return Results.BadRequest(new ApiErrorResponse("assignedMemberId does not belong to the conversation team"));
                }
            }

            var ticket = new Ticket
            {
                TeamId = conversation.TeamId,
                ProjectId = conversation.ConciergeApp!.ProjectId,
                ConciergeAppId = conversation.ConciergeAppId,
                CustomerId = conversation.CustomerId,
                ConversationId = conversation.Id,
                Title = title,
                Summary = summary,
                Category = "客户咨询",
                Status = TicketStatus.Pending,
                Priority = request.Priority,
                AssignedMemberId = assignedMember?.Id,
                LastActivityAt = DateTimeOffset.UtcNow,
            };

            dbContext.Tickets.Add(ticket);
            dbContext.TicketActivities.Add(new TicketActivity
            {
                TeamId = conversation.TeamId,
                Ticket = ticket,
                ActorMemberId = assignedMember?.Id,
                ActivityType = TicketActivityType.Created,
                Summary = "手动创建工单",
                Detail = summary,
            });
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/teams/{conversation.TeamId}/tickets/{ticket.Id}", ToTicketResponse(ticket));
        })
        .Produces<TicketResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("CreateTicket")
        .WithTags("Tickets");
    }
}
