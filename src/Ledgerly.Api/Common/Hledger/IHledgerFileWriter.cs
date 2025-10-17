using Ledgerly.Api.Common.Data.Entities;

namespace Ledgerly.Api.Common.Hledger;

/// <summary>
/// Interface for writing transactions to .hledger files.
/// Story 2.6: Created to enable testability of ConfirmImportHandler.
/// </summary>
public interface IHledgerFileWriter
{
    /// <summary>
    /// Appends a transaction to the hledger file using atomic write operations.
    /// </summary>
    Task<string> AppendTransactionAsync(
        Transaction transaction,
        string filePath,
        string? correlationId = null);

    /// <summary>
    /// Appends multiple transactions to the hledger file in a single atomic operation.
    /// </summary>
    Task<BulkWriteResult> BulkAppendAsync(
        List<Transaction> transactions,
        string filePath,
        string? correlationId = null);

    /// <summary>
    /// Restores the .hledger file from its .bak backup.
    /// </summary>
    void RestoreFromBackup(string filePath);
}
