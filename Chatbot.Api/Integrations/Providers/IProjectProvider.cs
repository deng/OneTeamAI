using Chatbot.Api.Integrations.Models;

namespace Chatbot.Api.Integrations.Providers;

public interface IProjectProvider : IExternalSystemAdapter
{
    Task<IReadOnlyList<IntegrationRecordRef>> ListProjectsAsync(
        IntegrationConnectionDescriptor connection,
        CancellationToken cancellationToken);
}
