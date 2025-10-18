namespace Ledgerly.Api.Common.Data.Entities;

/// <summary>
/// Audit entity for CSV import operations.
/// Story 2.6: Import Preview and Confirmation
/// Tracks import history for troubleshooting and analytics.
/// </summary>
public class CsvImport
{
    /// <summary>
    /// Unique identifier for this import operation.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Original CSV filename uploaded by user.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when import was executed.
    /// </summary>
    public DateTime ImportedAt { get; set; }

    /// <summary>
    /// Total number of rows found in CSV file.
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Number of transactions successfully written to .hledger file.
    /// </summary>
    public int SuccessfulImports { get; set; }

    /// <summary>
    /// Number of duplicate transactions skipped (from Story 2.5 duplicate detection).
    /// </summary>
    public int DuplicatesSkipped { get; set; }

    /// <summary>
    /// Number of parse or validation errors encountered.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Detected bank format identifier (e.g., "Chase", "BofA", "Custom").
    /// Null if not detected.
    /// </summary>
    public string? BankFormat { get; set; }

    /// <summary>
    /// JSON-serialized column mapping used for this import (from Story 2.4).
    /// Null if auto-detection was successful.
    /// </summary>
    public string? ColumnMapping { get; set; }

    /// <summary>
    /// SHA256 hash of the CSV file content for integrity verification.
    /// </summary>
    public string FileHash { get; set; } = string.Empty;

    /// <summary>
    /// User ID who performed the import.
    /// Used for authorization and audit trail.
    /// </summary>
    public Guid UserId { get; set; }
}
