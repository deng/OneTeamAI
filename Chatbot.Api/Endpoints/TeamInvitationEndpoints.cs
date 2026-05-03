partial class Program
{
    static void MapTeamInvitationEndpoints(WebApplication app)
    {
        // ── Invitations ──
        app.MapPost("/api/teams/{teamId:guid}/invitations", async (
            HttpContext httpContext,
            Guid teamId,
            CreateInvitationRequest request,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            var currentUser = session?.User;
            if (currentUser is null)
            {
                return UnauthorizedError();
            }

            var team = await dbContext.Teams
                .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);
            if (team is null)
            {
                return NotFoundError("team was not found", "team_not_found");
            }

            var email = request.Email.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequestError("email is required", "email_required");
            }

            if (!IsValidEmail(email))
            {
                return BadRequestError("email format is invalid", "invalid_email");
            }

            if (request.Role is MemberRole.Owner or MemberRole.AiEmployee)
            {
                return BadRequestError("role must be admin, operator, or viewer for invitations", "invalid_invitation_role");
            }

            var existingMember = await dbContext.Members
                .AnyAsync(x => x.TeamId == teamId && x.User != null && x.User.Email == email, cancellationToken);
            if (existingMember)
            {
                return Results.Conflict(new ApiErrorResponse("user is already a member of the team"));
            }

            var pendingInvitation = await dbContext.TeamInvitations
                .FirstOrDefaultAsync(
                    x => x.TeamId == teamId
                        && x.Email == email
                        && x.Status == InvitationStatus.Pending,
                    cancellationToken);

            if (pendingInvitation is not null && pendingInvitation.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return ConflictError("a pending invitation already exists for this email", "pending_invitation_exists");
            }

            var invitation = new TeamInvitation
            {
                TeamId = teamId,
                Email = email,
                Role = request.Role,
                Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
                Status = InvitationStatus.Pending,
                InvitedByUserId = currentUser.Id,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(Math.Clamp(request.ExpiresInDays, 1, 30)),
            };

            dbContext.TeamInvitations.Add(invitation);
            await dbContext.SaveChangesAsync(cancellationToken);

            invitation.Team = team;
            invitation.InvitedByUser = currentUser;

            return Results.Created($"/api/teams/{teamId}/invitations/{invitation.Id}", ToInvitationResponse(invitation));
        })
        .Produces<InvitationResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
        .WithName("CreateInvitation")
        .WithTags("Invitations");

        app.MapGet("/api/teams/{teamId:guid}/invitations", async (
            HttpContext httpContext,
            Guid teamId,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, teamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            var invitations = await dbContext.TeamInvitations
                .Where(x => x.TeamId == teamId)
                .Include(x => x.Team)
                .Include(x => x.InvitedByUser)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return Results.Ok(invitations.Select(ToInvitationResponse).ToList());
        })
        .Produces<List<InvitationResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .WithName("ListTeamInvitations")
        .WithTags("Invitations");

        app.MapGet("/api/invitations/me", async (
            HttpContext httpContext,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            var currentUser = session?.User;
            if (currentUser is null)
            {
                return UnauthorizedError();
            }

            var invitations = await dbContext.TeamInvitations
                .Where(x => x.Email == currentUser.Email && x.Status == InvitationStatus.Pending)
                .Include(x => x.Team)
                .Include(x => x.InvitedByUser)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            await ExpirePendingInvitationsAsync(dbContext, cancellationToken);

            return Results.Ok(invitations
                .Where(x => x.ExpiresAt > DateTimeOffset.UtcNow && x.Status == InvitationStatus.Pending)
                .Select(ToInvitationResponse)
                .ToList());
        })
        .Produces<List<InvitationResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .WithName("ListMyInvitations")
        .WithTags("Invitations");

        app.MapPost("/api/invitations/{invitationId:guid}/accept", async (
            HttpContext httpContext,
            Guid invitationId,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var session = await GetCurrentSessionAsync(httpContext, dbContext, cancellationToken);
            var currentUser = session?.User;
            if (currentUser is null)
            {
                return UnauthorizedError();
            }

            var invitation = await dbContext.TeamInvitations
                .Include(x => x.Team)
                .Include(x => x.InvitedByUser)
                .FirstOrDefaultAsync(x => x.Id == invitationId, cancellationToken);

            if (invitation is null)
            {
                return NotFoundError("invitation was not found", "invitation_not_found");
            }

            if (!string.Equals(invitation.Email, currentUser.Email, StringComparison.OrdinalIgnoreCase))
            {
                return ForbiddenError();
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return BadRequestError("invitation is not pending", "invitation_not_pending");
            }

            if (invitation.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                invitation.Status = InvitationStatus.Expired;
                invitation.RespondedAt = DateTimeOffset.UtcNow;
                invitation.UpdatedAt = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
                return Results.BadRequest(new ApiErrorResponse("invitation has expired"));
            }

            var existingMember = await dbContext.Members
                .FirstOrDefaultAsync(x => x.TeamId == invitation.TeamId && x.UserId == currentUser.Id, cancellationToken);

            if (existingMember is null)
            {
                dbContext.Members.Add(new Member
                {
                    TeamId = invitation.TeamId,
                    UserId = currentUser.Id,
                    MemberType = MemberType.Human,
                    Role = invitation.Role,
                    Status = MemberStatus.Active,
                    DisplayName = currentUser.DisplayName,
                    Title = invitation.Title,
                });
            }

            invitation.Status = InvitationStatus.Accepted;
            invitation.AcceptedByUserId = currentUser.Id;
            invitation.RespondedAt = DateTimeOffset.UtcNow;
            invitation.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToInvitationResponse(invitation));
        })
        .Produces<InvitationResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("AcceptInvitation")
        .WithTags("Invitations");

        app.MapPost("/api/invitations/{invitationId:guid}/revoke", async (
            HttpContext httpContext,
            Guid invitationId,
            AppDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var invitation = await dbContext.TeamInvitations
                .Include(x => x.Team)
                .Include(x => x.InvitedByUser)
                .FirstOrDefaultAsync(x => x.Id == invitationId, cancellationToken);

            if (invitation is null)
            {
                return Results.NotFound(new ApiErrorResponse("invitation was not found"));
            }

            var accessError = await EnsureTeamManagementAccessAsync(httpContext, dbContext, invitation.TeamId, cancellationToken);
            if (accessError is not null)
            {
                return accessError;
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return Results.BadRequest(new ApiErrorResponse("only pending invitations can be revoked"));
            }

            invitation.Status = InvitationStatus.Revoked;
            invitation.RespondedAt = DateTimeOffset.UtcNow;
            invitation.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToInvitationResponse(invitation));
        })
        .Produces<InvitationResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .WithName("RevokeInvitation")
        .WithTags("Invitations");
    }
}
