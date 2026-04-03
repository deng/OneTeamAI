using Chatbot.Api.Integrations.Models;

namespace Chatbot.Api.Integrations.Providers;

public interface ITicketProvider : IExternalSystemAdapter
{
    Task<IReadOnlyList<IntegrationRecordRef>> ListTicketsAsync(
        IntegrationConnectionDescriptor connection,
        CancellationToken cancellationToken);
}
