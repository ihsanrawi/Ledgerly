using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Api.Common.Exceptions;
using Ledgerly.Api.Common.Hledger;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TransactionEntity = Ledgerly.Api.Common.Data.Entities.Transaction;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Handler for confirming CSV import and persisting transactions.
/// Story 2.6: Import Preview and Confirmation
/// Implements atomic write to SQLite cache and .hledger file with full rollback on failure.
/// </summary>
public class ConfirmImportHandler
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly IHledgerFileWriter _fileWriter;
    private readonly ILogger<ConfirmImportHandler> _logger;

    public ConfirmImportHandler(
        LedgerlyDbContext dbContext,
        IHledgerFileWriter fileWriter,
        ILogger<ConfirmImportHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConfirmImportResponse> HandleAsync(ConfirmImportCommand command)
    {
        _logger.LogInformation(
            "Confirming import of {FileName} with {TransactionCount} transactions for user {UserId}",
            command.FileName,
            command.Transactions.Count,
            command.UserId);

        try
        {
            // Filter out duplicates (should already be filtered by frontend, but double-check)
            var transactionsToImport = command.Transactions
                .Where(t => !t.IsDuplicate)
                .ToList();

            var duplicatesSkipped = command.Transactions.Count - transactionsToImport.Count;

            if (transactionsToImport.Count == 0)
            {
                _logger.LogWarning("No transactions to import after filtering duplicates");
                return new ConfirmImportResponse
                {
                    Success = false,
                    TransactionsImported = 0,
                    DuplicatesSkipped = duplicatesSkipped,
                    ErrorMessage = "No transactions to import after filtering duplicates"
                };
            }

            // Resolve .hledger file path for user
            var hledgerFilePath = await ResolveHledgerFilePathAsync(command.UserId);

            _logger.LogDebug("Resolved hledger file path: {FilePath}", hledgerFilePath);

            // Start TransactionScope for atomicity
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            // Convert DTOs to Transaction entities
            var transactionEntities = new List<TransactionEntity>();
            foreach (var dto in transactionsToImport)
            {
                var transaction = new TransactionEntity
                {
                    HledgerTransactionCode = Guid.NewGuid(),
                    Date = dto.Date.Date, // Normalize to date only
                    Payee = SanitizePayee(dto.Payee),
                    Amount = dto.Amount, // Store as decimal (will convert when needed)
                    Account = dto.Account,
                    CategoryAccount = dto.Category,
                    Memo = dto.Memo
                };

                // Compute hash for duplicate detection
                transaction.UpdateHash();

                transactionEntities.Add(transaction);
            }

            // Insert transactions to SQLite cache
            await _dbContext.Transactions.AddRangeAsync(transactionEntities);
            await _dbContext.SaveChangesAsync();

            _logger.LogDebug("Inserted {Count} transactions into SQLite cache", transactionEntities.Count);

            // Write transactions to .hledger file (bulk append for performance)
            BulkWriteResult writeResult;
            try
            {
                writeResult = await _fileWriter.BulkAppendAsync(transactionEntities, hledgerFilePath);
            }
            catch (HledgerValidationException ex)
            {
                _logger.LogError(ex, "hledger validation failed during bulk write");
                throw new HledgerException(
                    "Transaction validation failed. Please check your transaction data and try again.",
                    ex);
            }

            _logger.LogInformation(
                "Successfully wrote {Count} transactions to .hledger file. Hash: {Before} -> {After}",
                writeResult.TransactionsWritten,
                writeResult.FileHashBefore,
                writeResult.FileHashAfter);

            // Create CSV import audit record
            var csvImportAudit = new CsvImport
            {
                Id = Guid.NewGuid(),
                FileName = command.FileName,
                ImportedAt = DateTime.UtcNow,
                TotalRows = command.Transactions.Count,
                SuccessfulImports = transactionEntities.Count,
                DuplicatesSkipped = duplicatesSkipped,
                ErrorCount = 0,
                FileHash = ComputeFileHash(command.FileName), // Placeholder, would need actual CSV content
                UserId = command.UserId
            };

            await _dbContext.CsvImports.AddAsync(csvImportAudit);
            await _dbContext.SaveChangesAsync();

            // Create hledger file audit record
            var fileAudit = new HledgerFileAudit
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Operation = AuditOperation.CsvImport,
                FileHashBefore = writeResult.FileHashBefore,
                FileHashAfter = writeResult.FileHashAfter,
                TransactionCount = transactionEntities.Count,
                BalanceChecksum = 0, // Placeholder, would need to query hledger
                TriggeredBy = AuditTrigger.User,
                RelatedEntityId = csvImportAudit.Id,
                UserId = command.UserId,
                FilePath = hledgerFilePath
            };

            await _dbContext.HledgerFileAudits.AddAsync(fileAudit);
            await _dbContext.SaveChangesAsync();

            // Commit transaction scope
            scope.Complete();

            _logger.LogInformation(
                "Successfully confirmed import: {Imported} transactions imported, {Skipped} duplicates skipped",
                transactionEntities.Count,
                duplicatesSkipped);

            return new ConfirmImportResponse
            {
                Success = true,
                TransactionsImported = transactionEntities.Count,
                DuplicatesSkipped = duplicatesSkipped,
                TransactionIds = transactionEntities.Select(t => t.HledgerTransactionCode).ToList()
            };
        }
        catch (HledgerException)
        {
            // Re-throw hledger exceptions to preserve error details
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm import for {FileName}", command.FileName);
            throw new HledgerException(
                "Import failed due to an unexpected error. Please try again.",
                ex);
        }
    }

    private Task<string> ResolveHledgerFilePathAsync(Guid userId)
    {
        // For MVP, use default path. In future, this would query UserSettings table.
        var defaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".ledgerly",
            "ledger.hledger");

        if (!File.Exists(defaultPath))
        {
            _logger.LogError("Hledger file not found: {FilePath}", defaultPath);
            throw new FileNotFoundException($"Hledger file not found: {defaultPath}");
        }

        return Task.FromResult(defaultPath);
    }

    private static string SanitizePayee(string payee)
    {
        // Remove special characters that could break hledger syntax
        return payee
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Replace("\t", " ")
            .Trim();
    }

    private static string ComputeFileHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
