using Chatbot.Api.Options;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Chatbot.Api.Services;

public sealed class ChatbotAgentRuntime : IWorkflowTextCompletionService
{
    private readonly AIHostAgent _hostAgent;
    private readonly ChatClient _chatClient;
    private readonly string _baseInstructions;

    public ChatbotAgentRuntime(IOptions<ChatbotOptions> options, ILoggerFactory loggerFactory)
    {
        var settings = options.Value;
        _baseInstructions = settings.Instructions;
        var openAiOptions = new OpenAIClientOptions();
        var blockchainQueryService = new BlockchainTransactionQueryService();

        if (!string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            openAiOptions.Endpoint = new Uri(settings.Endpoint);
        }

        _chatClient = new ChatClient(
            settings.Model,
            new ApiKeyCredential(settings.ApiKey),
            openAiOptions);

        var agent = _chatClient.AsAIAgent(
            settings.Instructions,
            settings.AgentName,
            settings.Description,
            tools:
            [
                AIFunctionFactory.Create(
                    blockchainQueryService.GetTransactionByIdAsync,
                    new AIFunctionFactoryOptions
                    {
                        Name = "query_blockchain_transaction",
                        Description = "根据区块链交易 ID 查询交易详情，返回结构化交易数据。结果中可能包含数据来源和 mock 标记，回答时应据此说明数据性质。"
                    })
            ],
            clientFactory: null,
            loggerFactory,
            services: null);

        _hostAgent = new AIHostAgent(agent, new InMemoryAgentSessionStore());
    }

    public AIAgent Agent => _hostAgent;

    public ValueTask<AgentSession> GetOrCreateSessionAsync(string conversationId, CancellationToken cancellationToken) =>
        _hostAgent.GetOrCreateSessionAsync(conversationId, cancellationToken);

    public ValueTask SaveSessionAsync(string conversationId, AgentSession session, CancellationToken cancellationToken) =>
        _hostAgent.SaveSessionAsync(conversationId, session, cancellationToken);

    public async Task<string> CompleteAsync(
        string prompt,
        string? sessionId,
        CancellationToken cancellationToken)
    {
        var effectiveSessionId = string.IsNullOrWhiteSpace(sessionId)
            ? Guid.NewGuid().ToString("N")
            : sessionId.Trim();
        var session = await GetOrCreateSessionAsync(effectiveSessionId, cancellationToken);
        var chunks = new List<string>();

        await foreach (var update in Agent.RunStreamingAsync(prompt, session, cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(update.Text))
            {
                chunks.Add(update.Text);
            }
        }

        await SaveSessionAsync(effectiveSessionId, session, cancellationToken);
        return string.Concat(chunks).Trim();
    }

    public async Task<string> CompleteTextAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken)
    {
        var completion = await _chatClient.CompleteChatAsync(
            [
                new SystemChatMessage($"{_baseInstructions}\n\n{systemPrompt}".Trim()),
                new UserChatMessage(userPrompt)
            ],
            new ChatCompletionOptions
            {
                MaxOutputTokenCount = 600,
            },
            cancellationToken);

        return string.Concat(completion.Value.Content.Select(part => part.Text ?? string.Empty)).Trim();
    }
}
