namespace Ledgerly.Api.Common.Hledger;

/// <summary>
/// Result from hledger balance command.
/// </summary>
public class HledgerBalanceResult
{
    public required List<BalanceEntry> Balances { get; init; }
    public required decimal TotalBalance { get; init; }
}

public class BalanceEntry
{
    public required string Account { get; init; }
    public required decimal Amount { get; init; }
    public required string Commodity { get; init; }
}
