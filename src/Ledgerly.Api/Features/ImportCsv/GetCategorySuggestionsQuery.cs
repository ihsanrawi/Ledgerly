using Ledgerly.Contracts.Dtos;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine query for getting category suggestions based on ImportRules (Story 2.5).
/// Matches transaction payees against ImportRule patterns to suggest categories.
/// </summary>
public record GetCategorySuggestionsQuery
{
    /// <summary>
    /// Parsed transactions to get category suggestions for.
    /// </summary>
    public List<ParsedTransactionDto> Transactions { get; init; } = new();
}

/// <summary>
/// Response containing category suggestions for each transaction.
/// </summary>
public record GetCategorySuggestionsResponse
{
    /// <summary>
    /// Category suggestions for each transaction index.
    /// </summary>
    public List<CategorySuggestionDto> Suggestions { get; init; } = new();
}
