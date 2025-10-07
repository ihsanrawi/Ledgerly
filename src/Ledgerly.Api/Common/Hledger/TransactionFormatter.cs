using System.Text;
using Ledgerly.Api.Common.Data.Entities;

namespace Ledgerly.Api.Common.Hledger;

/// <summary>
/// Formats Transaction entities into valid hledger file syntax.
/// </summary>
public class TransactionFormatter
{
    private const int AmountColumnAlignment = 52;
    private const string PostingIndent = "  "; // 2 spaces per hledger standard

    /// <summary>
    /// Formats a transaction into hledger file format.
    /// </summary>
    /// <param name="transaction">The transaction to format.</param>
    /// <returns>Formatted hledger transaction string.</returns>
    /// <example>
    /// Output format:
    /// 2025-01-15 (550e8400-e29b-41d4-a716-446655440000) Whole Foods
    ///   Expenses:Groceries    $45.23
    ///   Assets:Checking
    /// </example>
    public string FormatTransaction(Transaction transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        ValidateTransaction(transaction);

        var sb = new StringBuilder();

        // Line 1: Date, transaction code (Guid), payee, optional memo
        sb.Append(transaction.Date.ToString("yyyy-MM-dd"));
        sb.Append($" ({transaction.HledgerTransactionCode})");
        sb.Append($" {transaction.Payee}");

        if (!string.IsNullOrWhiteSpace(transaction.Memo))
        {
            sb.Append($" | {transaction.Memo}");
        }

        sb.AppendLine();

        // Line 2: Category account with amount (aligned at column 52)
        sb.Append(PostingIndent);
        sb.Append(transaction.CategoryAccount);
        sb.Append(AlignAmount(transaction.CategoryAccount, FormatAmount(transaction.Amount)));
        sb.AppendLine();

        // Line 3: Primary account (amount inferred by hledger for balance)
        sb.Append(PostingIndent);
        sb.AppendLine(transaction.Account);

        return sb.ToString();
    }

    /// <summary>
    /// Formats multiple transactions with blank line separators.
    /// </summary>
    public string FormatTransactions(IEnumerable<Transaction> transactions)
    {
        if (transactions == null)
        {
            throw new ArgumentNullException(nameof(transactions));
        }

        var sb = new StringBuilder();
        var first = true;

        foreach (var transaction in transactions)
        {
            if (!first)
            {
                sb.AppendLine(); // Blank line between transactions
            }

            sb.Append(FormatTransaction(transaction));
            first = false;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Extracts unique account names from a transaction.
    /// </summary>
    public IEnumerable<string> GetAccountsFromTransaction(Transaction transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        var accounts = new HashSet<string>();

        if (!string.IsNullOrWhiteSpace(transaction.Account))
        {
            accounts.Add(transaction.Account);
        }

        if (!string.IsNullOrWhiteSpace(transaction.CategoryAccount))
        {
            accounts.Add(transaction.CategoryAccount);
        }

        return accounts;
    }

    private static void ValidateTransaction(Transaction transaction)
    {
        if (string.IsNullOrWhiteSpace(transaction.Payee))
        {
            throw new ArgumentException("Transaction payee cannot be empty.", nameof(transaction));
        }

        if (string.IsNullOrWhiteSpace(transaction.Account))
        {
            throw new ArgumentException("Transaction account cannot be empty.", nameof(transaction));
        }

        if (string.IsNullOrWhiteSpace(transaction.CategoryAccount))
        {
            throw new ArgumentException("Transaction category account cannot be empty.", nameof(transaction));
        }

        if (transaction.HledgerTransactionCode == Guid.Empty)
        {
            throw new ArgumentException("Transaction code cannot be empty GUID.", nameof(transaction));
        }
    }

    private static string FormatAmount(decimal amount)
    {
        // Format with $ prefix and 2 decimal places
        return $"${amount:F2}";
    }

    private static string AlignAmount(string accountName, string formattedAmount)
    {
        var currentLength = PostingIndent.Length + accountName.Length;
        var spacesNeeded = AmountColumnAlignment - currentLength;

        if (spacesNeeded < 2)
        {
            spacesNeeded = 2; // Minimum 2 spaces between account and amount
        }

        return new string(' ', spacesNeeded) + formattedAmount;
    }
}
