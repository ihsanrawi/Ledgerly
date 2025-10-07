using System.Security.Cryptography;
using System.Text;
using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Api.Common.Exceptions;
using Serilog;
using Serilog.Context;

namespace Ledgerly.Api.Common.Hledger;

/// <summary>
/// Writes transactions to .hledger files with atomic operations and backup support.
/// </summary>
public class HledgerFileWriter
{
    private readonly HledgerProcessRunner _processRunner;
    private readonly TransactionFormatter _formatter;
    private readonly ILogger<HledgerFileWriter> _logger;
    private const int MaxRetries = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(100);

    public HledgerFileWriter(
        HledgerProcessRunner processRunner,
        TransactionFormatter formatter,
        ILogger<HledgerFileWriter> logger)
    {
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Appends a transaction to the hledger file using atomic write operations.
    /// Creates backup before writing, validates with hledger check, and rolls back on failure.
    /// </summary>
    /// <param name="transaction">The transaction to append.</param>
    /// <param name="filePath">Path to the .hledger file.</param>
    /// <param name="correlationId">Optional correlation ID for tracing (will generate if not provided).</param>
    /// <returns>SHA256 hash of the file after successful write.</returns>
    public async Task<string> AppendTransactionAsync(
        Transaction transaction,
        string filePath,
        string? correlationId = null)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));
        }

        var actualCorrelationId = correlationId ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty("CorrelationId", actualCorrelationId))
        {
            _logger.LogInformation(
                "Appending transaction {TransactionCode} to {FilePath}",
                transaction.HledgerTransactionCode,
                filePath);

            return await AppendTransactionWithRetryAsync(transaction, filePath);
        }
    }

    /// <summary>
    /// Restores the .hledger file from its .bak backup.
    /// </summary>
    public void RestoreFromBackup(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));
        }

        var backupPath = GetBackupPath(filePath);

        if (!File.Exists(backupPath))
        {
            throw new FileNotFoundException($"Backup file not found: {backupPath}");
        }

        _logger.LogWarning("Restoring {FilePath} from backup {BackupPath}", filePath, backupPath);

        File.Copy(backupPath, filePath, overwrite: true);

        _logger.LogInformation("Successfully restored {FilePath} from backup", filePath);
    }

    private async Task<string> AppendTransactionWithRetryAsync(Transaction transaction, string filePath)
    {
        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                return await AppendTransactionInternalAsync(transaction, filePath);
            }
            catch (IOException ex) when (attempt < MaxRetries)
            {
                _logger.LogWarning(
                    ex,
                    "File I/O error on attempt {Attempt}/{MaxRetries}, retrying after {Delay}ms",
                    attempt,
                    MaxRetries,
                    RetryDelay.TotalMilliseconds);

                await Task.Delay(RetryDelay);
            }
        }
    }

    private async Task<string> AppendTransactionInternalAsync(Transaction transaction, string filePath)
    {
        var tempPath = GetTempPath(filePath);
        var backupPath = GetBackupPath(filePath);

        try
        {
            // Read existing file content
            var existingContent = await ReadExistingFileAsync(filePath);

            // Calculate SHA256 hash before
            var hashBefore = CalculateSha256(existingContent);

            _logger.LogDebug("File hash before write: {Hash}", hashBefore);

            // Parse existing account declarations
            var existingAccounts = ParseAccountDeclarations(existingContent);

            // Get accounts from new transaction
            var newAccounts = _formatter.GetAccountsFromTransaction(transaction);

            // Merge accounts (add new ones)
            var allAccounts = new HashSet<string>(existingAccounts);
            foreach (var account in newAccounts)
            {
                allAccounts.Add(account);
            }

            // Format transaction
            var formattedTransaction = _formatter.FormatTransaction(transaction);

            // Build new file content: declarations + existing content + new transaction
            var newContent = BuildFileContent(allAccounts, existingContent, formattedTransaction);

            // Write to temp file
            await File.WriteAllTextAsync(tempPath, newContent, Encoding.UTF8);

            _logger.LogDebug("Wrote transaction to temp file: {TempPath}", tempPath);

            // Validate with hledger check
            await ValidateTempFileAsync(tempPath);

            // Create backup of original file (if it exists)
            if (File.Exists(filePath))
            {
                File.Copy(filePath, backupPath, overwrite: true);
                _logger.LogDebug("Created backup: {BackupPath}", backupPath);
            }

            // Atomic rename: temp -> actual
            File.Move(tempPath, filePath, overwrite: true);

            _logger.LogDebug("Atomically moved temp file to: {FilePath}", filePath);

            // Calculate SHA256 hash after
            var hashAfter = CalculateSha256(newContent);

            _logger.LogInformation(
                "Successfully appended transaction {TransactionCode}. Hash: {HashBefore} -> {HashAfter}",
                transaction.HledgerTransactionCode,
                hashBefore,
                hashAfter);

            return hashAfter;
        }
        catch (HledgerValidationException)
        {
            // Validation failed, clean up temp file
            CleanupTempFile(tempPath);
            throw;
        }
        catch (Exception ex)
        {
            // Unexpected error, clean up temp file
            CleanupTempFile(tempPath);

            _logger.LogError(ex, "Failed to append transaction to {FilePath}", filePath);
            throw;
        }
    }

    private async Task<string> ReadExistingFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            // File doesn't exist, return empty content
            return string.Empty;
        }

        return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
    }

    private async Task ValidateTempFileAsync(string tempPath)
    {
        try
        {
            var validationResult = await _processRunner.ValidateFile(tempPath);

            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("; ", validationResult.Errors);

                _logger.LogError(
                    "hledger validation failed for {TempPath}: {Errors}",
                    tempPath,
                    errorMessage);

                throw new HledgerValidationException(
                    "Transaction validation failed",
                    errorMessage,
                    tempPath);
            }

            _logger.LogDebug("hledger validation passed for {TempPath}", tempPath);
        }
        catch (HledgerValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during validation of {TempPath}", tempPath);
            throw new HledgerValidationException(
                "Validation failed due to unexpected error",
                ex.Message,
                tempPath);
        }
    }

    private static HashSet<string> ParseAccountDeclarations(string content)
    {
        var accounts = new HashSet<string>();

        if (string.IsNullOrWhiteSpace(content))
        {
            return accounts;
        }

        var lines = content.Split('\n', StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("account ", StringComparison.Ordinal))
            {
                var accountName = line.Substring(8).Trim(); // Remove "account " prefix
                if (!string.IsNullOrWhiteSpace(accountName))
                {
                    accounts.Add(accountName);
                }
            }
        }

        return accounts;
    }

    private static string BuildFileContent(
        HashSet<string> accounts,
        string existingContent,
        string formattedTransaction)
    {
        var sb = new StringBuilder();

        // Add account declarations at the top
        if (accounts.Count > 0)
        {
            var sortedAccounts = accounts.OrderBy(a => a).ToList();

            foreach (var account in sortedAccounts)
            {
                sb.AppendLine($"account {account}");
            }

            sb.AppendLine(); // Blank line after declarations
        }

        // Add existing transactions (skip old account declarations)
        if (!string.IsNullOrWhiteSpace(existingContent))
        {
            var transactionsContent = RemoveAccountDeclarations(existingContent);

            if (!string.IsNullOrWhiteSpace(transactionsContent))
            {
                sb.Append(transactionsContent);

                // Ensure there's a blank line before new transaction
                if (!transactionsContent.EndsWith("\n\n"))
                {
                    sb.AppendLine();
                }
            }
        }

        // Add new transaction
        sb.Append(formattedTransaction);

        return sb.ToString();
    }

    private static string RemoveAccountDeclarations(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var lines = content.Split('\n');
        var inDeclarationsSection = true;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip account declaration lines at the start
            if (inDeclarationsSection)
            {
                if (trimmedLine.StartsWith("account ", StringComparison.Ordinal))
                {
                    continue; // Skip declaration line
                }

                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue; // Skip blank lines in declarations section
                }

                // Found non-declaration content, exit declarations section
                inDeclarationsSection = false;
            }

            sb.AppendLine(line);
        }

        return sb.ToString().TrimStart();
    }

    private static string CalculateSha256(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static string GetTempPath(string filePath)
    {
        return $"{filePath}.tmp";
    }

    private static string GetBackupPath(string filePath)
    {
        return $"{filePath}.bak";
    }

    private void CleanupTempFile(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
                _logger.LogDebug("Deleted temp file: {TempPath}", tempPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete temp file: {TempPath}", tempPath);
        }
    }
}
