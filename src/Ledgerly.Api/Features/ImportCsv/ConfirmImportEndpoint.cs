using Wolverine.Http;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine HTTP endpoint for confirming CSV import after preview.
/// Story 2.6: Import Preview and Confirmation
/// TODO Phase 2: Add authentication to ensure user can only import to their own .hledger file.
/// </summary>
public class ConfirmImportEndpoint
{
    /// <summary>
    /// Confirm and execute CSV import after user preview and edits.
    /// Persists transactions to SQLite cache and .hledger file atomically.
    /// </summary>
    /// <param name="command">Import confirmation command with transaction list</param>
    /// <param name="handler">Confirm import handler (injected)</param>
    /// <returns>Import confirmation response with success status and transaction IDs</returns>
    [WolverinePost("/api/import/confirm")]
    // TODO Phase 2: Add [Authorize] attribute when authentication is implemented
    public async Task<ConfirmImportResponse> Post(
        ConfirmImportCommand command,
        ConfirmImportHandler handler)
    {
        // TODO: Extract UserId from authenticated context (ClaimsPrincipal)
        // For MVP, using command.UserId directly, but in production this should be:
        // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // command = command with { UserId = Guid.Parse(userId) };

        var result = await handler.HandleAsync(command);
        return result;
    }
}
