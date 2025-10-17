using Wolverine;
using Wolverine.Http;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine HTTP endpoint for creating new ImportRules (Story 2.5).
/// </summary>
public class CreateImportRuleEndpoint
{
    /// <summary>
    /// Create new ImportRule from user-corrected categorization.
    /// </summary>
    /// <param name="command">Create rule command</param>
    /// <param name="bus">Wolverine message bus (injected)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response with created rule ID</returns>
    [WolverinePost("/api/categorize/create-rule")]
    public async Task<CreateImportRuleResponse> Post(
        CreateImportRuleCommand command,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<CreateImportRuleResponse>(command, ct);
        return result;
    }
}
