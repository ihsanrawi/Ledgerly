using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Data.Entities;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine handler for creating new ImportRule (Story 2.5).
/// Creates rule with initial confidence 0.6 and priority 1 (highest).
/// </summary>
public class CreateImportRuleHandler
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<CreateImportRuleHandler> _logger;

    public CreateImportRuleHandler(
        LedgerlyDbContext dbContext,
        ILogger<CreateImportRuleHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CreateImportRuleResponse> Handle(
        CreateImportRuleCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Creating new ImportRule: Pattern={Pattern}, Category={Category}, MatchType={MatchType}",
            command.PayeePattern, command.SuggestedCategory, command.MatchType);

        // Validate inputs
        if (string.IsNullOrWhiteSpace(command.PayeePattern))
        {
            throw new ArgumentException("Payee pattern cannot be empty", nameof(command.PayeePattern));
        }

        if (string.IsNullOrWhiteSpace(command.SuggestedCategory))
        {
            throw new ArgumentException("Suggested category cannot be empty", nameof(command.SuggestedCategory));
        }

        // Create new ImportRule
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = command.PayeePattern.Trim(),
            SuggestedCategory = command.SuggestedCategory.Trim(),
            MatchType = command.MatchType,
            Priority = 1, // Highest priority for user-created rules
            Confidence = 0.6m, // Initial confidence for new user-created rule
            TimesApplied = 1,
            TimesAccepted = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow
        };

        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "ImportRule created successfully: RuleId={RuleId}, Pattern={Pattern}, Category={Category}",
            rule.Id, rule.PayeePattern, rule.SuggestedCategory);

        return new CreateImportRuleResponse
        {
            RuleId = rule.Id,
            Message = "Rule created successfully"
        };
    }
}
