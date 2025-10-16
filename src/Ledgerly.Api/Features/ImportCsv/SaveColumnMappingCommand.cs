namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine command to save user's manual column mapping for future CSV imports.
/// Story 2.4 - Manual Column Mapping Interface (AC: 5)
/// </summary>
public record SaveColumnMappingCommand
{
    /// <summary>
    /// User-provided bank identifier (e.g., "Chase Checking", "Bank of America").
    /// Max 100 characters, alphanumeric + spaces/hyphens only (security: prevent XSS/CSV injection).
    /// </summary>
    public string BankIdentifier { get; init; } = string.Empty;

    /// <summary>
    /// Dictionary mapping CSV header names to field types.
    /// Key: CSV header name (e.g., "Trans Date")
    /// Value: Field type (date, amount, description, memo, balance, account, debit, credit)
    /// </summary>
    public Dictionary<string, string> ColumnMappings { get; init; } = new();

    /// <summary>
    /// Optional filename pattern for automatic bank matching (e.g., "*chase*.csv").
    /// Used as fallback if header signature matching fails.
    /// </summary>
    public string? FileNamePattern { get; init; }

    /// <summary>
    /// CSV header signature for exact matching (ordered list of all headers).
    /// Primary criterion for bank matching (100% confidence).
    /// Filename pattern used as fallback only.
    /// </summary>
    public string[] HeaderSignature { get; init; } = Array.Empty<string>();
}
