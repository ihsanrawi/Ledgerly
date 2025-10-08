namespace Ledgerly.Contracts.Dtos;

/// <summary>
/// Represents an account balance with hierarchical children.
/// Used for rendering account trees in the UI.
/// </summary>
public record BalanceDto
{
    /// <summary>
    /// Full account path (e.g., "Assets:Checking", "Expenses:Groceries")
    /// </summary>
    public string Account { get; init; } = string.Empty;

    /// <summary>
    /// Current balance for this account
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Depth in account hierarchy (1 = root, 2 = first level child, etc.)
    /// </summary>
    public int Depth { get; init; }

    /// <summary>
    /// Child accounts in the hierarchy
    /// </summary>
    public List<BalanceDto> Children { get; init; } = new();
}
