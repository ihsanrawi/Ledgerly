namespace Ledgerly.Api.Common.Data.Entities;

/// <summary>
/// Audit entity tracking all modifications to .hledger files.
/// Story 2.6: Import Preview and Confirmation
/// Provides comprehensive audit trail for file changes and consistency checks.
/// </summary>
public class HledgerFileAudit
{
    /// <summary>
    /// Unique identifier for this audit record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp of the file modification.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Type of operation performed on the .hledger file.
    /// </summary>
    public AuditOperation Operation { get; set; }

    /// <summary>
    /// SHA256 hash of file content before modification.
    /// Used for integrity verification and change tracking.
    /// </summary>
    public string FileHashBefore { get; set; } = string.Empty;

    /// <summary>
    /// SHA256 hash of file content after modification.
    /// Used for integrity verification and change tracking.
    /// </summary>
    public string FileHashAfter { get; set; } = string.Empty;

    /// <summary>
    /// Total number of transactions in file after operation.
    /// Used for consistency checks.
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Sum of all account balances after operation.
    /// Used for double-entry bookkeeping consistency verification.
    /// Should always be zero in valid double-entry system.
    /// </summary>
    public decimal BalanceChecksum { get; set; }

    /// <summary>
    /// What triggered this file modification.
    /// </summary>
    public AuditTrigger TriggeredBy { get; set; }

    /// <summary>
    /// Error message if operation failed.
    /// Null if operation succeeded.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Related entity ID (Transaction ID, CsvImport ID, etc.).
    /// Used to link audit record to originating entity.
    /// </summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>
    /// User ID who initiated the operation.
    /// Used for authorization and audit trail.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Full file path that was modified.
    /// Useful for multi-file scenarios in future.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
}

/// <summary>
/// Types of operations that can modify .hledger files.
/// </summary>
public enum AuditOperation
{
    CsvImport = 0,
    TransactionAdd = 1,
    TransactionEdit = 2,
    TransactionDelete = 3,
    ExternalEdit = 4,
    CacheRebuild = 5
}

/// <summary>
/// What triggered the file modification.
/// </summary>
public enum AuditTrigger
{
    User = 0,      // User-initiated action via UI
    System = 1,    // System-initiated (scheduled task, cache sync)
    External = 2   // External edit detected by FileSystemWatcher
}
