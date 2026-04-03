using Chatbot.Api.Integrations.Models;

namespace Chatbot.Api.Integrations.Providers;

public interface IFileKnowledgeProvider : IExternalSystemAdapter
{
    Task<IReadOnlyList<FileKnowledgeRecord>> ListFilesAsync(
        IntegrationConnectionDescriptor connection,
        string? folderPath,
        CancellationToken cancellationToken);
}
