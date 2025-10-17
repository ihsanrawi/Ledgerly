using Ledgerly.Contracts.Dtos;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine query for detecting duplicate transactions (Story 2.5).
/// Accepts parsed CSV transactions and returns list of duplicates found in database.
/// </summary>
public record DetectDuplicatesQuery
{
    /// <summary>
    /// Parsed transactions from CSV to check for duplicates.
    /// </summary>
    public List<ParsedTransactionDto> Transactions { get; init; } = new();
}

/// <summary>
/// Parsed transaction DTO for duplicate detection.
/// Minimal fields required for hash computation and matching.
/// </summary>
public record ParsedTransactionDto
{
    /// <summary>
    /// Transaction date.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Payee/merchant name.
    /// </summary>
    public string Payee { get; init; } = string.Empty;

    /// <summary>
    /// Transaction amount.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Row index in CSV (for correlating duplicates back to CSV rows).
    /// </summary>
    public int RowIndex { get; init; }
}

/// <summary>
/// Response containing detected duplicate transactions.
/// </summary>
public record DetectDuplicatesResponse
{
    /// <summary>
    /// List of duplicate transactions found in database.
    /// </summary>
    public List<DuplicateTransactionDto> Duplicates { get; init; } = new();
}
