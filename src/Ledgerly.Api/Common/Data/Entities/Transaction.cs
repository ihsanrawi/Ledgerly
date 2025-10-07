namespace Ledgerly.Api.Common.Data.Entities;

/// <summary>
/// Represents a financial transaction.
/// NOTE: This is a minimal model for Story 1.4 (file writer MVP).
/// Full entity with EF Core mapping will be implemented in future stories.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Unique identifier embedded in .hledger file as transaction code.
    /// </summary>
    public Guid HledgerTransactionCode { get; set; }

    /// <summary>
    /// Transaction date (YYYY-MM-DD format in hledger).
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Payee/merchant name (e.g., "Whole Foods", "Amazon").
    /// </summary>
    public string Payee { get; set; } = string.Empty;

    /// <summary>
    /// Transaction amount (positive for expenses/income).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Primary account (e.g., "Assets:Checking").
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// Category account for the expense/income (e.g., "Expenses:Groceries").
    /// </summary>
    public string CategoryAccount { get; set; } = string.Empty;

    /// <summary>
    /// Optional memo/description.
    /// </summary>
    public string? Memo { get; set; }
}
