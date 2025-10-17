namespace Ledgerly.Contracts.Dtos;

/// <summary>
/// DTO representing a category suggestion for a transaction based on ImportRule pattern matching (Story 2.5).
/// </summary>
public record CategorySuggestionDto
{
    /// <summary>
    /// Index in CSV parsed transactions array.
    /// Used to associate suggestion with correct transaction row.
    /// </summary>
    public int TransactionIndex { get; init; }

    /// <summary>
    /// ID of the matched ImportRule that generated this suggestion.
    /// Used for accepting suggestions and updating confidence.
    /// </summary>
    public Guid RuleId { get; init; }

    /// <summary>
    /// Suggested category account (e.g., "Expenses:Groceries").
    /// </summary>
    public string SuggestedCategory { get; init; } = string.Empty;

    /// <summary>
    /// Confidence score (0.0-1.0).
    /// Calculated as TimesAccepted / TimesApplied.
    /// </summary>
    public decimal Confidence { get; init; }

    /// <summary>
    /// The payee pattern that matched (e.g., "WHOLE FOODS").
    /// Shown to user for transparency.
    /// </summary>
    public string MatchedPattern { get; init; } = string.Empty;
}
