using System.ComponentModel;

namespace Chatbot.Api.Services;

public sealed class BlockchainTransactionQueryService
{
    [Description("根据区块链交易 ID 查询交易详情，返回结构化交易数据。")]
    public Task<BlockchainTransactionQueryResult> GetTransactionByIdAsync(
        [Description("要查询的区块链交易 ID。")] string transactionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = new BlockchainTransactionQueryResult(
            TransactionId: transactionId,
            Status: "mocked_success",
            Network: "ethereum-sepolia",
            BlockNumber: 12345678,
            Confirmations: 12,
            Amount: "0.1234 ETH",
            FromAddress: "0x1111111111111111111111111111111111111111",
            ToAddress: "0x2222222222222222222222222222222222222222",
            IsMock: true,
            DataSource: "mock",
            Message: "这是区块链交易查询工具的预设返回值，后续可替换为真实链上查询。");

        return Task.FromResult(result);
    }
}

public sealed record BlockchainTransactionQueryResult(
    string TransactionId,
    string Status,
    string Network,
    long BlockNumber,
    int Confirmations,
    string Amount,
    string FromAddress,
    string ToAddress,
    bool IsMock,
    string DataSource,
    string Message);
