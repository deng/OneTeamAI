using Chatbot.Api.Domain.Enums;
using Chatbot.Api.Integrations.Models;

namespace Chatbot.Api.Integrations.Providers;

public interface IExternalSystemAdapter
{
    ExternalSystemType ExternalSystemType { get; }

    bool CanHandle(ExternalSystemType externalSystemType);

    IntegrationConnectionDescriptor BuildDescriptor(Domain.Entities.IntegrationConnection connection);

    Task<IntegrationConnectionTestResult> TestConnectionAsync(
        IntegrationConnectionDescriptor connection,
        CancellationToken cancellationToken);
}
