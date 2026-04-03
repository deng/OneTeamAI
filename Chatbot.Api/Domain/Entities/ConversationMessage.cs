using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class ConversationMessage : EntityBase
{
    public Guid ConversationId { get; set; }

    public Conversation? Conversation { get; set; }

    public ConversationParticipantType ParticipantType { get; set; }

    public Guid? MemberId { get; set; }

    public Member? Member { get; set; }

    [MaxLength(64)]
    public string? SenderName { get; set; }

    public string Content { get; set; } = string.Empty;
}
