namespace Chatbot.Api.Services;

public interface IWorkflowTextCompletionService
{
    Task<string> CompleteTextAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken);
}
