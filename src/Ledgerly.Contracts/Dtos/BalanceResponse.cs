namespace Ledgerly.Contracts.Dtos;

/// <summary>
/// API response wrapper for balance queries.
/// Includes timestamp for client cache management.
/// </summary>
public record BalanceResponse
{
    /// <summary>
    /// Hierarchical balance tree (root accounts with children)
    /// </summary>
    public List<BalanceDto> Balances { get; init; } = new();

    /// <summary>
    /// Timestamp when balance was calculated
    /// </summary>
    public DateTime AsOfDate { get; init; }
}
