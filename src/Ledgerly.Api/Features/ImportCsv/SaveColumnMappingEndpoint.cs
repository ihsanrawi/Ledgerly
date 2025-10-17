using Wolverine;
using Wolverine.Http;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine HTTP endpoint for saving column mapping rules.
/// Story 2.4 - Manual Column Mapping Interface (AC: 5, 6).
/// </summary>
public class SaveColumnMappingEndpoint
{
    /// <summary>
    /// Save column mapping rule for CSV imports.
    /// </summary>
    /// <param name="command">Save column mapping command</param>
    /// <param name="bus">Wolverine message bus (injected)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response with saved mapping ID and message</returns>
    [WolverinePost("/api/import/save-mapping")]
    public async Task<Ledgerly.Contracts.Dtos.SaveColumnMappingResponse> Post(
        SaveColumnMappingCommand command,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Ledgerly.Contracts.Dtos.SaveColumnMappingResponse>(command, ct);
        return result;
    }
}
