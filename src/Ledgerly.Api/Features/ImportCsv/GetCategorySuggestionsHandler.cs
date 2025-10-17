using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine handler for category suggestion based on ImportRule pattern matching (Story 2.5).
/// Implements priority-ordered pattern matching with confidence scoring.
/// </summary>
public class GetCategorySuggestionsHandler
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<GetCategorySuggestionsHandler> _logger;

    public GetCategorySuggestionsHandler(
        LedgerlyDbContext dbContext,
        ILogger<GetCategorySuggestionsHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<GetCategorySuggestionsResponse> Handle(
        GetCategorySuggestionsQuery query,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Getting category suggestions for {Count} transactions",
            query.Transactions.Count);

        var suggestions = new List<CategorySuggestionDto>();

        // Load all active ImportRules ordered by Priority (cache-friendly)
        var rules = await _dbContext.ImportRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

        if (rules.Count == 0)
        {
            _logger.LogWarning("No active ImportRules found for category suggestions");
            return new GetCategorySuggestionsResponse { Suggestions = suggestions };
        }

        // Match each transaction against rules
        foreach (var transaction in query.Transactions)
        {
            var suggestion = GetSuggestionForTransaction(transaction, rules);
            if (suggestion != null)
            {
                suggestions.Add(suggestion);
            }
        }

        // NOTE: No SaveChangesAsync needed - TimesApplied tracking moved to AcceptCategorySuggestionHandler
        // (data integrity fix - Story 2.5 QA)

        _logger.LogInformation(
            "Category suggestion complete. {SuggestionCount} suggestions out of {TotalCount} transactions",
            suggestions.Count, query.Transactions.Count);

        return new GetCategorySuggestionsResponse { Suggestions = suggestions };
    }

    /// <summary>
    /// Get category suggestion for a single transaction by matching against ImportRules.
    /// Returns first match (highest priority rule wins).
    /// NOTE: Does NOT modify ImportRule - TimesApplied tracking moved to AcceptCategorySuggestionHandler.
    /// </summary>
    private CategorySuggestionDto? GetSuggestionForTransaction(
        ParsedTransactionDto transaction,
        List<ImportRule> rules)
    {
        var normalizedPayee = transaction.Payee.Trim().ToLowerInvariant();

        foreach (var rule in rules)
        {
            var normalizedPattern = rule.PayeePattern.ToLowerInvariant();
            bool isMatch = rule.MatchType switch
            {
                Ledgerly.Api.Common.Data.Entities.MatchType.Exact => normalizedPayee == normalizedPattern,
                Ledgerly.Api.Common.Data.Entities.MatchType.Contains => normalizedPayee.Contains(normalizedPattern),
                Ledgerly.Api.Common.Data.Entities.MatchType.StartsWith => normalizedPayee.StartsWith(normalizedPattern),
                Ledgerly.Api.Common.Data.Entities.MatchType.EndsWith => normalizedPayee.EndsWith(normalizedPattern),
                _ => false
            };

            if (isMatch)
            {
                // NOTE: TimesApplied is NOT incremented here (data integrity fix - Story 2.5 QA).
                // TimesApplied is incremented only when user accepts the suggestion (AcceptCategorySuggestionHandler).
                // This prevents permanently inaccurate confidence scores when users reject suggestions.

                _logger.LogDebug(
                    "Category suggestion matched: Payee={Payee}, Pattern={Pattern}, Category={Category}, Confidence={Confidence}",
                    transaction.Payee, rule.PayeePattern, rule.SuggestedCategory, rule.Confidence);

                return new CategorySuggestionDto
                {
                    TransactionIndex = transaction.RowIndex,
                    RuleId = rule.Id,
                    SuggestedCategory = rule.SuggestedCategory,
                    Confidence = rule.Confidence,
                    MatchedPattern = rule.PayeePattern
                };
            }
        }

        return null; // No match found
    }
}
