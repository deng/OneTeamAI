using Chatbot.Api.Integrations.Models;

namespace Chatbot.Api.Integrations.Providers;

public interface ICustomerProvider : IExternalSystemAdapter
{
    Task<IReadOnlyList<IntegrationRecordRef>> ListCustomersAsync(
        IntegrationConnectionDescriptor connection,
        CancellationToken cancellationToken);
}
