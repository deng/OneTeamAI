using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class Conversation : EntityBase
{
    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    public Guid ConciergeAppId { get; set; }

    public ConciergeApp? ConciergeApp { get; set; }

    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public ConversationStatus Status { get; set; } = ConversationStatus.Open;

    public ICollection<ConversationMessage> Messages { get; set; } = [];

    public ICollection<Ticket> Tickets { get; set; } = [];
}
