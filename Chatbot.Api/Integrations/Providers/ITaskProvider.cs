using Chatbot.Api.Integrations.Models;

namespace Chatbot.Api.Integrations.Providers;

public interface ITaskProvider : IExternalSystemAdapter
{
    Task<IReadOnlyList<IntegrationRecordRef>> ListTasksAsync(
        IntegrationConnectionDescriptor connection,
        CancellationToken cancellationToken);
}
