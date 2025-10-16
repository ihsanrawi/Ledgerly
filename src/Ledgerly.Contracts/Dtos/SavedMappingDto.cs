namespace Ledgerly.Contracts.Dtos;

/// <summary>
/// DTO for saved column mapping rule.
/// Story 2.4 - Manual Column Mapping Interface (AC: 6).
/// </summary>
public record SavedMappingDto
{
    /// <summary>
    /// Unique identifier of the mapping rule.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// User-provided bank identifier (e.g., "Chase Checking", "Bank of America").
    /// </summary>
    public string BankIdentifier { get; init; } = string.Empty;

    /// <summary>
    /// Dictionary mapping CSV header names to field types.
    /// Key: CSV header name (e.g., "Trans Date")
    /// Value: Field type (date, amount, description, memo, balance, account, debit, credit)
    /// </summary>
    public Dictionary<string, string> ColumnMappings { get; init; } = new();

    /// <summary>
    /// Timestamp when mapping was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp when mapping was last used.
    /// </summary>
    public DateTime LastUsedAt { get; init; }

    /// <summary>
    /// Number of times this mapping has been used.
    /// </summary>
    public int TimesUsed { get; init; }
}
