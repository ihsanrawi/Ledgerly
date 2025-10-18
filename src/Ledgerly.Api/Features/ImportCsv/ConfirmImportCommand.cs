namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Command to confirm and execute CSV import after user preview and edits.
/// Story 2.6: Import Preview and Confirmation
/// </summary>
public record ConfirmImportCommand
{
    /// <summary>
    /// List of transactions to import (after user edits to payee and category).
    /// Transactions marked as duplicates should be filtered out by the frontend.
    /// </summary>
    public List<ImportTransactionDto> Transactions { get; init; } = new();

    /// <summary>
    /// Original CSV filename for audit trail.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// User ID for authorization and file path resolution.
    /// Injected from authenticated context, not from request body.
    /// </summary>
    public Guid UserId { get; init; }
}

/// <summary>
/// DTO representing a transaction to import from CSV.
/// Frontend sends decimal amounts, backend converts to Money value object.
/// </summary>
public record ImportTransactionDto
{
    public DateTime Date { get; init; }
    public string Payee { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Account { get; init; } = string.Empty;
    public string? Memo { get; init; }
    public bool IsDuplicate { get; init; }
}

/// <summary>
/// Response after successful import confirmation.
/// </summary>
public record ConfirmImportResponse
{
    public bool Success { get; init; }
    public int TransactionsImported { get; init; }
    public int DuplicatesSkipped { get; init; }
    public List<Guid> TransactionIds { get; init; } = new();
    public string? ErrorMessage { get; init; }
}
