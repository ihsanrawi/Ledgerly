using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine handler for duplicate transaction detection (Story 2.5).
/// Computes transaction hashes and queries database for matches.
/// </summary>
public class DetectDuplicatesHandler
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<DetectDuplicatesHandler> _logger;

    public DetectDuplicatesHandler(
        LedgerlyDbContext dbContext,
        ILogger<DetectDuplicatesHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<DetectDuplicatesResponse> Handle(DetectDuplicatesQuery query, CancellationToken ct)
    {
        _logger.LogInformation(
            "Detecting duplicates for {Count} transactions",
            query.Transactions.Count);

        var duplicates = new List<DuplicateTransactionDto>();

        // Compute hashes for all transactions
        var transactionHashes = query.Transactions
            .Select(t => new
            {
                Transaction = t,
                Hash = Transaction.ComputeTransactionHash(t.Date, t.Payee, t.Amount)
            })
            .ToList();

        // Log computed hashes for debugging
        foreach (var th in transactionHashes)
        {
            _logger.LogInformation(
                "CSV Transaction: Date={Date}, Payee={Payee}, Amount={Amount}, ComputedHash={Hash}",
                th.Transaction.Date, th.Transaction.Payee, th.Transaction.Amount, th.Hash);
        }

        // Batch query for duplicates (performance optimization)
        var hashes = transactionHashes.Select(th => th.Hash).ToList();
        var existingTransactions = await _dbContext.Transactions
            .Where(t => hashes.Contains(t.Hash))
            .ToListAsync(ct);

        _logger.LogInformation(
            "Found {ExistingCount} existing transactions in database matching {HashCount} hashes",
            existingTransactions.Count, hashes.Count);

        // Log existing transaction hashes from database
        foreach (var existing in existingTransactions)
        {
            _logger.LogInformation(
                "DB Transaction: Date={Date}, Payee={Payee}, Amount={Amount}, Hash={Hash}",
                existing.Date, existing.Payee, existing.Amount, existing.Hash);
        }

        // Create lookup dictionary for O(1) access
        var existingByHash = existingTransactions
            .GroupBy(t => t.Hash)
            .ToDictionary(g => g.Key, g => g.First());

        // Match hashes to find duplicates
        foreach (var th in transactionHashes)
        {
            if (existingByHash.TryGetValue(th.Hash, out var existing))
            {
                duplicates.Add(new DuplicateTransactionDto
                {
                    TransactionId = existing.HledgerTransactionCode,
                    Date = existing.Date,
                    Payee = existing.Payee,
                    Amount = existing.Amount,
                    Category = existing.CategoryAccount,
                    Account = existing.Account,
                    CsvRowIndex = th.Transaction.RowIndex
                });

                _logger.LogDebug(
                    "Duplicate found: Payee={Payee}, Date={Date}, Amount={Amount}, RowIndex={RowIndex}",
                    existing.Payee, existing.Date, existing.Amount, th.Transaction.RowIndex);
            }
        }

        _logger.LogInformation(
            "Duplicate detection complete. Found {DuplicateCount} duplicates out of {TotalCount} transactions",
            duplicates.Count, query.Transactions.Count);

        return new DetectDuplicatesResponse
        {
            Duplicates = duplicates
        };
    }
}
