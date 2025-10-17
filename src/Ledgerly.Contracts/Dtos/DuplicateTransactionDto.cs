namespace Ledgerly.Contracts.Dtos;

/// <summary>
/// DTO representing a duplicate transaction match for Story 2.5.
/// Returned when duplicate detection finds an existing transaction with matching hash.
/// </summary>
public record DuplicateTransactionDto
{
    /// <summary>
    /// Unique identifier of the existing matched transaction.
    /// </summary>
    public Guid TransactionId { get; init; }

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
    /// Category account (e.g., "Expenses:Groceries").
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Primary account (e.g., "Assets:Checking").
    /// </summary>
    public string Account { get; init; } = string.Empty;

    /// <summary>
    /// Index of the CSV row that matches this duplicate.
    /// Used to correlate duplicate with parsed CSV data.
    /// </summary>
    public int CsvRowIndex { get; init; }
}
