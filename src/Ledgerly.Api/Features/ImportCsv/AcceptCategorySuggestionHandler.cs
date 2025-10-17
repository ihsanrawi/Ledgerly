using Ledgerly.Api.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine handler for accepting category suggestions (Story 2.5).
/// Updates ImportRule confidence score: TimesAccepted / TimesApplied.
/// </summary>
public class AcceptCategorySuggestionHandler
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<AcceptCategorySuggestionHandler> _logger;

    public AcceptCategorySuggestionHandler(
        LedgerlyDbContext dbContext,
        ILogger<AcceptCategorySuggestionHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<AcceptCategorySuggestionResponse> Handle(
        AcceptCategorySuggestionCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Accepting category suggestion for rule: {RuleId}",
            command.RuleId);

        var rule = await _dbContext.ImportRules.FindAsync(new object[] { command.RuleId }, ct);

        if (rule == null)
        {
            _logger.LogWarning("ImportRule not found: {RuleId}", command.RuleId);
            throw new InvalidOperationException($"ImportRule {command.RuleId} not found");
        }

        // Increment TimesAccepted AND TimesApplied (data integrity fix - Story 2.5 QA)
        // TimesApplied is incremented here when user accepts, not during suggestion phase
        rule.TimesAccepted++;
        rule.TimesApplied++;

        // Update last used timestamp
        rule.LastUsedAt = DateTime.UtcNow;

        // Recalculate confidence: TimesAccepted / TimesApplied
        // Confidence is automatically capped at 1.0 by division
        rule.Confidence = rule.TimesApplied > 0
            ? (decimal)rule.TimesAccepted / rule.TimesApplied
            : 0.0m;

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Category suggestion accepted. Rule: {RuleId}, Pattern: {Pattern}, NewConfidence: {Confidence}, TimesAccepted: {TimesAccepted}, TimesApplied: {TimesApplied}",
            rule.Id, rule.PayeePattern, rule.Confidence, rule.TimesAccepted, rule.TimesApplied);

        return new AcceptCategorySuggestionResponse
        {
            Success = true,
            NewConfidence = rule.Confidence
        };
    }
}
