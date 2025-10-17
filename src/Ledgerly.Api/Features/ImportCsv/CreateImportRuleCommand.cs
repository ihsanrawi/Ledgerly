using Ledgerly.Api.Common.Data.Entities;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine command for creating new ImportRule from user-corrected categorization (Story 2.5).
/// </summary>
public record CreateImportRuleCommand
{
    /// <summary>
    /// Payee pattern to match (e.g., "WHOLE FOODS").
    /// </summary>
    public string PayeePattern { get; init; } = string.Empty;

    /// <summary>
    /// Suggested category account (e.g., "Expenses:Groceries").
    /// </summary>
    public string SuggestedCategory { get; init; } = string.Empty;

    /// <summary>
    /// Type of pattern matching (Exact, Contains, StartsWith, EndsWith).
    /// </summary>
    public Ledgerly.Api.Common.Data.Entities.MatchType MatchType { get; init; } = Ledgerly.Api.Common.Data.Entities.MatchType.Contains;
}

/// <summary>
/// Response for creating ImportRule.
/// </summary>
public record CreateImportRuleResponse
{
    /// <summary>
    /// ID of the created ImportRule.
    /// </summary>
    public Guid RuleId { get; init; }

    /// <summary>
    /// Success message.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
