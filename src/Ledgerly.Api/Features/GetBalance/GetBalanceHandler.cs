using Ledgerly.Api.Common.Hledger;
using Ledgerly.Contracts.Dtos;

namespace Ledgerly.Api.Features.GetBalance;

/// <summary>
/// Wolverine handler for GetBalanceQuery.
/// Executes hledger bal -O json and transforms flat results into hierarchical tree.
/// </summary>
public class GetBalanceHandler
{
    private readonly IHledgerProcessRunner _processRunner;
    private readonly ILogger<GetBalanceHandler> _logger;

    public GetBalanceHandler(
        IHledgerProcessRunner processRunner,
        ILogger<GetBalanceHandler> logger)
    {
        _processRunner = processRunner;
        _logger = logger;
    }

    public async Task<BalanceResponse> Handle(GetBalanceQuery query, CancellationToken ct)
    {
        _logger.LogInformation(
            "Executing GetBalanceQuery. AccountFilter: {AccountFilter}",
            query.AccountFilter);

        // For Story 1.5, use seed.hledger for testing
        // TODO: Story 2.x - Replace with user's actual ledger file path
        var seedFilePath = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "TestData", "seed.hledger");
        seedFilePath = Path.GetFullPath(seedFilePath);

        _logger.LogDebug("Using hledger file: {FilePath}", seedFilePath);

        // Parse account filter (comma-separated string to array)
        var accounts = query.AccountFilter?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Execute hledger bal -O json
        var result = await _processRunner.GetBalances(seedFilePath, accounts);

        // Transform flat list to hierarchical tree
        var balances = BuildHierarchy(result.Balances);

        // Calculate parent balances (sum of children for stub nodes)
        CalculateParentBalances(balances);

        _logger.LogInformation(
            "GetBalanceQuery completed with {Count} root accounts",
            balances.Count);

        return new BalanceResponse
        {
            Balances = balances,
            AsOfDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Converts flat hledger balance list to hierarchical tree structure.
    /// Handles missing parent accounts (zero balance) by creating stub nodes.
    /// </summary>
    private List<BalanceDto> BuildHierarchy(List<BalanceEntry> flatList)
    {
        // Step 1: Create dictionary of all accounts from hledger output
        var accountDict = new Dictionary<string, BalanceDto>();

        foreach (var item in flatList)
        {
            var dto = new BalanceDto
            {
                Account = item.Account,
                Balance = item.Amount,
                Depth = item.Account.Split(':').Length,
                Children = new List<BalanceDto>()
            };
            accountDict[item.Account] = dto;
        }

        // Step 2: Build parent-child relationships
        var rootAccounts = new List<BalanceDto>();

        // Take snapshot of keys to avoid modification during enumeration
        var accountKeys = accountDict.Keys.ToList();

        foreach (var key in accountKeys)
        {
            var kvp = new KeyValuePair<string, BalanceDto>(key, accountDict[key]);
            var parts = kvp.Key.Split(':');

            if (parts.Length == 1)
            {
                // Root-level account (e.g., "Assets", "Expenses", "Income")
                rootAccounts.Add(kvp.Value);
            }
            else
            {
                // Child account - find or create parent
                var parentPath = string.Join(':', parts.SkipLast(1));

                if (accountDict.TryGetValue(parentPath, out var parent))
                {
                    // Parent exists in hledger output
                    parent.Children.Add(kvp.Value);
                }
                else
                {
                    // Parent not in hledger output (zero balance) - create stub node
                    var stub = new BalanceDto
                    {
                        Account = parentPath,
                        Balance = 0,
                        Depth = parts.Length - 1,
                        Children = new List<BalanceDto> { kvp.Value }
                    };
                    accountDict[parentPath] = stub;

                    // Recursively attach stub to its parent (handles multi-level gaps)
                    AttachToParentRecursive(stub, accountDict, rootAccounts);
                }
            }
        }

        return rootAccounts;
    }

    /// <summary>
    /// Recursively attaches account to parent hierarchy.
    /// Creates stub nodes for missing parents.
    /// </summary>
    private void AttachToParentRecursive(
        BalanceDto dto,
        Dictionary<string, BalanceDto> accountDict,
        List<BalanceDto> rootAccounts)
    {
        var parts = dto.Account.Split(':');

        if (parts.Length == 1)
        {
            // This is a root account
            if (!rootAccounts.Contains(dto))
            {
                rootAccounts.Add(dto);
            }
            return;
        }

        // Find or create parent
        var parentPath = string.Join(':', parts.SkipLast(1));

        if (accountDict.TryGetValue(parentPath, out var parent))
        {
            // Parent exists - attach if not already attached
            if (!parent.Children.Contains(dto))
            {
                parent.Children.Add(dto);
            }
        }
        else
        {
            // Parent doesn't exist - create stub and recurse
            var parentStub = new BalanceDto
            {
                Account = parentPath,
                Balance = 0,
                Depth = parts.Length - 1,
                Children = new List<BalanceDto> { dto }
            };
            accountDict[parentPath] = parentStub;

            // Recursively attach parent stub
            AttachToParentRecursive(parentStub, accountDict, rootAccounts);
        }
    }

    /// <summary>
    /// Calculates parent account balances by summing children.
    /// Recursively processes tree from leaves to root.
    /// </summary>
    private void CalculateParentBalances(List<BalanceDto> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.Children.Count > 0)
            {
                // Recursively calculate children first (bottom-up)
                CalculateParentBalances(node.Children);

                // If this is a stub node (balance = 0) with children, sum children balances
                if (node.Balance == 0)
                {
                    node.Balance = node.Children.Sum(c => c.Balance);
                }
            }
        }
    }
}
