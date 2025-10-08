using Ledgerly.Contracts.Dtos;
using Wolverine;
using Wolverine.Http;

namespace Ledgerly.Api.Features.GetBalance;

/// <summary>
/// Wolverine HTTP endpoint for balance queries.
/// Dispatches GetBalanceQuery to Wolverine message bus.
/// </summary>
public class GetBalanceEndpoint
{
    /// <summary>
    /// Retrieves account balances with optional filtering.
    /// </summary>
    /// <param name="accounts">Optional comma-separated account filter (e.g., "Assets,Expenses")</param>
    /// <param name="bus">Wolverine message bus (injected)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Balance response with hierarchical account tree</returns>
    [WolverineGet("/api/balance")]
    public async Task<BalanceResponse> Get(
        string? accounts,
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new GetBalanceQuery(accounts);
        var result = await bus.InvokeAsync<BalanceResponse>(query, ct);
        return result;
    }
}
