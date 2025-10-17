namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine command for accepting a category suggestion (Story 2.5).
/// Updates ImportRule confidence score based on user acceptance.
/// </summary>
public record AcceptCategorySuggestionCommand
{
    /// <summary>
    /// ID of the ImportRule that was accepted.
    /// </summary>
    public Guid RuleId { get; init; }
}

/// <summary>
/// Response for accepting category suggestion.
/// </summary>
public record AcceptCategorySuggestionResponse
{
    /// <summary>
    /// Success indicator.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Updated confidence score after acceptance.
    /// </summary>
    public decimal NewConfidence { get; init; }
}
