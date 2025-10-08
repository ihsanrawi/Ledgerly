namespace Ledgerly.Api.Features.GetBalance;

/// <summary>
/// Query for retrieving account balances.
/// Wolverine message handled by GetBalanceHandler.
/// </summary>
public record GetBalanceQuery(string? AccountFilter = null);
