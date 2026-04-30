using Chatbot.Api.Services;
using System.Collections.Concurrent;

namespace Chatbot.Api.Tests;

public sealed class WorkflowTextCompletionStub : IWorkflowTextCompletionService
{
    private readonly ConcurrentQueue<string> _responses = new();

    public int CallCount { get; private set; }

    public void Reset(params string[] responses)
    {
        while (_responses.TryDequeue(out _))
        {
        }

        CallCount = 0;

        foreach (var response in responses)
        {
            _responses.Enqueue(response);
        }
    }

    public Task<string> CompleteTextAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;

        if (!_responses.TryDequeue(out var response))
        {
            throw new InvalidOperationException("No workflow text stub response configured.");
        }

        return Task.FromResult(response);
    }
}
