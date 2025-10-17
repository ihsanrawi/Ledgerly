using System.Security.Cryptography;
using System.Text;

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

    /// <summary>
    /// SHA256 hash computed from Date + Payee + Amount for duplicate detection (Story 2.5).
    /// Format: Base64-encoded SHA256 hash of "{Date:yyyy-MM-dd}|{Payee.ToLowerInvariant()}|{Amount}".
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Computes transaction hash for duplicate detection.
    /// Uses SHA256 hash of normalized transaction key: date + payee + amount.
    /// </summary>
    public static string ComputeTransactionHash(DateTime date, string payee, decimal amount)
    {
        var normalizedPayee = payee.Trim().ToLowerInvariant();
        var hashInput = $"{date:yyyy-MM-dd}|{normalizedPayee}|{amount}";

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(hashInput));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Computes and updates the Hash property for this transaction.
    /// Should be called before persisting transaction to database.
    /// </summary>
    public void UpdateHash()
    {
        Hash = ComputeTransactionHash(Date, Payee, Amount);
    }
}
