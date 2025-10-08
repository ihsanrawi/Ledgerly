namespace Ledgerly.Api.Common.Hledger;

/// <summary>
/// Interface for hledger process execution.
/// Enables mocking in unit tests.
/// </summary>
public interface IHledgerProcessRunner
{
    /// <summary>
    /// Executes hledger balance command and returns parsed results.
    /// </summary>
    Task<HledgerBalanceResult> GetBalances(string hledgerFilePath, string[]? accounts = null);

    /// <summary>
    /// Validates hledger file using 'hledger check'.
    /// </summary>
    Task<ValidationResult> ValidateFile(string hledgerFilePath);

    /// <summary>
    /// Generic CLI execution wrapper.
    /// </summary>
    Task<string> ExecuteCommand(string[] args, TimeSpan? timeout = null);
}
