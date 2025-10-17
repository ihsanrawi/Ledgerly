using Wolverine;
using Wolverine.Http;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine HTTP endpoint for duplicate transaction detection (Story 2.5).
/// </summary>
public class DetectDuplicatesEndpoint
{
    /// <summary>
    /// Detect duplicate transactions from parsed CSV data.
    /// </summary>
    /// <param name="query">Detect duplicates query with parsed transactions</param>
    /// <param name="bus">Wolverine message bus (injected)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response containing list of duplicate transactions found</returns>
    [WolverinePost("/api/import/detect-duplicates")]
    public async Task<DetectDuplicatesResponse> Post(
        DetectDuplicatesQuery query,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<DetectDuplicatesResponse>(query, ct);
        return result;
    }
}
