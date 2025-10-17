using Wolverine;
using Wolverine.Http;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine HTTP endpoint for deleting column mapping rules.
/// Story 2.4 - Manual Column Mapping Interface (AC: 7).
/// </summary>
public class DeleteColumnMappingEndpoint
{
    /// <summary>
    /// Delete a saved column mapping rule.
    /// </summary>
    /// <param name="id">Mapping ID to delete</param>
    /// <param name="bus">Wolverine message bus (injected)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content on success</returns>
    [WolverineDelete("/api/import/mappings/{id}")]
    public async Task<IResult> Delete(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        await bus.InvokeAsync(new DeleteColumnMappingCommand { Id = id }, ct);
        return Results.NoContent();
    }
}
