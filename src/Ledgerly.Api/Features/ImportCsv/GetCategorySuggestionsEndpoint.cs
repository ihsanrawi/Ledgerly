using Wolverine;
using Wolverine.Http;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine HTTP endpoint for category suggestions (Story 2.5).
/// </summary>
public class GetCategorySuggestionsEndpoint
{
    /// <summary>
    /// Get category suggestions for parsed CSV transactions.
    /// </summary>
    /// <param name="query">Query with parsed transactions</param>
    /// <param name="bus">Wolverine message bus (injected)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response containing category suggestions</returns>
    [WolverinePost("/api/categorize/suggestions")]
    public async Task<GetCategorySuggestionsResponse> Post(
        GetCategorySuggestionsQuery query,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<GetCategorySuggestionsResponse>(query, ct);
        return result;
    }
}
