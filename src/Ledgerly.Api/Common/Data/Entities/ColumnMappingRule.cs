using System.ComponentModel.DataAnnotations;

namespace Ledgerly.Api.Common.Data.Entities;

/// <summary>
/// Represents a saved column mapping rule for automatic CSV import.
/// Story 2.4 - Manual Column Mapping Interface.
/// </summary>
public class ColumnMappingRule
{
    /// <summary>
    /// Unique identifier for the mapping rule.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// User-provided bank identifier (e.g., "Chase Checking", "Bank of America").
    /// Max 100 characters, alphanumeric + spaces/hyphens only.
    /// Indexed for fast lookup.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string BankIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Optional filename pattern for automatic bank matching (e.g., "*chase*.csv").
    /// Used as fallback if header signature matching fails.
    /// </summary>
    [MaxLength(200)]
    public string? BankMatchPattern { get; set; }

    /// <summary>
    /// CSV header signature for exact matching (stored as JSON array).
    /// Primary criterion for bank matching (100% confidence).
    /// Ordered list of all headers from original CSV.
    /// </summary>
    [Required]
    public string HeaderSignature { get; set; } = string.Empty;

    /// <summary>
    /// Column mappings (stored as JSON dictionary).
    /// Key: CSV header name (e.g., "Trans Date")
    /// Value: Field type (date, amount, description, memo, balance, account, debit, credit)
    /// </summary>
    [Required]
    public string ColumnMappings { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when mapping was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when mapping was last used in an import.
    /// Updated each time mapping is applied to a CSV.
    /// </summary>
    [Required]
    public DateTime LastUsedAt { get; set; }

    /// <summary>
    /// Counter for how many times this mapping has been used.
    /// Helps identify popular vs. unused mappings for cleanup.
    /// </summary>
    [Required]
    public int TimesUsed { get; set; }

    /// <summary>
    /// Flag indicating if mapping is active (soft delete).
    /// Inactive mappings not used in auto-matching.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Row version for optimistic concurrency control.
    /// Prevents race conditions when multiple users/sessions update same mapping.
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
