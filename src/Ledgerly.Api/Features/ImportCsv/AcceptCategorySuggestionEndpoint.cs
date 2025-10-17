using Wolverine;
using Wolverine.Http;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine HTTP endpoint for accepting category suggestions (Story 2.5).
/// </summary>
public class AcceptCategorySuggestionEndpoint
{
    /// <summary>
    /// Accept a category suggestion and update ImportRule confidence.
    /// </summary>
    /// <param name="command">Accept suggestion command</param>
    /// <param name="bus">Wolverine message bus (injected)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response with success status and new confidence</returns>
    [WolverinePost("/api/categorize/accept")]
    public async Task<AcceptCategorySuggestionResponse> Post(
        AcceptCategorySuggestionCommand command,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<AcceptCategorySuggestionResponse>(command, ct);
        return result;
    }
}
