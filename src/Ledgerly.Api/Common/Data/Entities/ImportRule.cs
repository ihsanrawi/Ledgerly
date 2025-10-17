namespace Ledgerly.Api.Common.Data.Entities;

/// <summary>
/// Represents a category suggestion rule for CSV imports (Story 2.5).
/// Matches transaction payees against patterns to suggest categories.
/// Learning system: confidence improves as users accept suggestions.
/// </summary>
public class ImportRule
{
    /// <summary>
    /// Unique identifier for the rule.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Pattern to match against transaction payee (e.g., "WHOLE FOODS", "AMAZON").
    /// Case-insensitive matching.
    /// </summary>
    public string PayeePattern { get; set; } = string.Empty;

    /// <summary>
    /// Type of pattern matching to perform.
    /// </summary>
    public MatchType MatchType { get; set; }

    /// <summary>
    /// Rule application order (1 = highest priority).
    /// When multiple rules match, highest priority wins.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Suggested category account (e.g., "Expenses:Groceries").
    /// </summary>
    public string SuggestedCategory { get; set; } = string.Empty;

    /// <summary>
    /// Accuracy score (0.0-1.0), calculated as TimesAccepted / TimesApplied.
    /// Initial value: 0.6 for user-created rules, varies for system-seeded rules.
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// Usage count (incremented every time rule matched).
    /// </summary>
    public int TimesApplied { get; set; }

    /// <summary>
    /// User confirmation count (incremented when user accepts suggestion).
    /// </summary>
    public int TimesAccepted { get; set; }

    /// <summary>
    /// Rule active state. Inactive rules are not used for matching.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Rule creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last time rule was matched (null if never used).
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
}

/// <summary>
/// Pattern matching types for ImportRule payee matching.
/// NOTE: Regex match type EXCLUDED from MVP to prevent ReDoS attacks.
/// </summary>
public enum MatchType
{
    /// <summary>
    /// Exact string match (case-insensitive).
    /// </summary>
    Exact = 0,

    /// <summary>
    /// Payee contains pattern (case-insensitive).
    /// </summary>
    Contains = 1,

    /// <summary>
    /// Payee starts with pattern (case-insensitive).
    /// </summary>
    StartsWith = 2,

    /// <summary>
    /// Payee ends with pattern (case-insensitive).
    /// </summary>
    EndsWith = 3
}
