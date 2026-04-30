using Chatbot.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Api.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<Member> Members => Set<Member>();

    public DbSet<AIMemberProfile> AiMemberProfiles => Set<AIMemberProfile>();

    public DbSet<AiMemberTemplate> AiMemberTemplates => Set<AiMemberTemplate>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    public DbSet<ConciergeApp> ConciergeApps => Set<ConciergeApp>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketActivity> TicketActivities => Set<TicketActivity>();

    public DbSet<AgentWorkflowRun> AgentWorkflowRuns => Set<AgentWorkflowRun>();

    public DbSet<AgentWorkflowStep> AgentWorkflowSteps => Set<AgentWorkflowStep>();

    public DbSet<AgentExecutionLog> AgentExecutionLogs => Set<AgentExecutionLog>();

    public DbSet<IntegrationConnection> IntegrationConnections => Set<IntegrationConnection>();

    public DbSet<TeamInvitation> TeamInvitations => Set<TeamInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
