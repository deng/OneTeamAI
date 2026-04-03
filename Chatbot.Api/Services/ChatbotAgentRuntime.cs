using Chatbot.Api.Options;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Chatbot.Api.Services;

public sealed class ChatbotAgentRuntime
{
    private readonly AIHostAgent _hostAgent;

    public ChatbotAgentRuntime(IOptions<ChatbotOptions> options, ILoggerFactory loggerFactory)
    {
        var settings = options.Value;
        var openAiOptions = new OpenAIClientOptions();
        var blockchainQueryService = new BlockchainTransactionQueryService();

        if (!string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            openAiOptions.Endpoint = new Uri(settings.Endpoint);
        }

        var chatClient = new ChatClient(
            settings.Model,
            new ApiKeyCredential(settings.ApiKey),
            openAiOptions);

        var agent = chatClient.AsAIAgent(
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
}
