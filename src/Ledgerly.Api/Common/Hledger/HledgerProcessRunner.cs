using System.Diagnostics;
using System.Text.Json;
using Ledgerly.Api.Common.Exceptions;
using Serilog;
using Serilog.Context;

namespace Ledgerly.Api.Common.Hledger;

/// <summary>
/// Executes hledger CLI commands via Process, captures output, parses responses, handles errors.
/// </summary>
public class HledgerProcessRunner
{
    private readonly HledgerBinaryManager _binaryManager;
    private readonly ILogger<HledgerProcessRunner> _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);

    public HledgerProcessRunner(
        HledgerBinaryManager binaryManager,
        ILogger<HledgerProcessRunner> logger,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _binaryManager = binaryManager;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Executes hledger balance command and returns parsed results.
    /// </summary>
    public async Task<HledgerBalanceResult> GetBalances(string hledgerFilePath, string[]? accounts = null)
    {
        if (!File.Exists(hledgerFilePath))
        {
            throw new FileNotFoundException($"hledger file not found: {hledgerFilePath}");
        }

        var args = new List<string> { "bal", "-f", hledgerFilePath, "-O", "json" };
        if (accounts != null && accounts.Length > 0)
        {
            args.AddRange(accounts);
        }

        var output = await ExecuteCommand(args.ToArray());

        try
        {
            // Parse JSON output from hledger
            //
            // hledger JSON Format Documentation:
            // The output is a 2-element array: [accountsData, totalsData]
            //
            // accountsData structure: Array of account tuples
            // Each tuple: [
            //   "account name",           // string - full account path (e.g., "Assets:Checking")
            //   "account name",           // string - repeated for compatibility
            //   depth,                    // number - account hierarchy depth (1 for top-level)
            //   [amounts]                 // array of amount objects
            // ]
            //
            // Amount object structure:
            // {
            //   "acommodity": "USD",                    // currency/commodity symbol
            //   "aquantity": {                          // quantity with precision
            //     "decimalMantissa": 104580,            // mantissa for exact decimal representation
            //     "decimalPlaces": 2,                   // number of decimal places
            //     "floatingPoint": 1045.80              // floating-point representation (use this for display)
            //   }
            // }
            //
            // Example JSON:
            // [
            //   [
            //     ["Assets:Checking", "Assets:Checking", 1, [{"acommodity": "$", "aquantity": {"floatingPoint": 5045.80}}]],
            //     ["Expenses:Groceries", "Expenses:Groceries", 1, [{"acommodity": "$", "aquantity": {"floatingPoint": 52.30}}]]
            //   ],
            //   [{"acommodity": "$", "aquantity": {"floatingPoint": 0}}]  // totals (balanced ledger = 0)
            // ]
            var jsonDoc = JsonDocument.Parse(output);
            var balances = new List<BalanceEntry>();
            decimal totalBalance = 0;

            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array &&
                jsonDoc.RootElement.GetArrayLength() >= 1)
            {
                var accountsArray = jsonDoc.RootElement[0];

                if (accountsArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var accountTuple in accountsArray.EnumerateArray())
                    {
                        // Each tuple: ["account", "account", depth, [amounts]]
                        if (accountTuple.ValueKind == JsonValueKind.Array &&
                            accountTuple.GetArrayLength() >= 4)
                        {
                            var account = accountTuple[0].GetString() ?? "";
                            var amountsArray = accountTuple[3];

                            if (amountsArray.ValueKind == JsonValueKind.Array &&
                                amountsArray.GetArrayLength() > 0)
                            {
                                var firstAmount = amountsArray[0];
                                var commodity = firstAmount.GetProperty("acommodity").GetString() ?? "USD";
                                var quantity = firstAmount.GetProperty("aquantity");
                                var floatingPoint = quantity.GetProperty("floatingPoint").GetDecimal();

                                balances.Add(new BalanceEntry
                                {
                                    Account = account,
                                    Amount = floatingPoint,
                                    Commodity = commodity
                                });

                                totalBalance += floatingPoint;
                            }
                        }
                    }
                }
            }

            return new HledgerBalanceResult
            {
                Balances = balances,
                TotalBalance = totalBalance
            };
        }
        catch (JsonException ex)
        {
            throw new HledgerParseException($"Failed to parse hledger balance output", output, ex);
        }
    }

    /// <summary>
    /// Validates hledger file using 'hledger check'.
    /// </summary>
    public async Task<ValidationResult> ValidateFile(string hledgerFilePath)
    {
        if (!File.Exists(hledgerFilePath))
        {
            return ValidationResult.Failure($"File not found: {hledgerFilePath}");
        }

        try
        {
            await ExecuteCommand(new[] { "check", "-f", hledgerFilePath });
            return ValidationResult.Success();
        }
        catch (HledgerProcessException ex) when (ex.ExitCode == 1)
        {
            // Exit code 1 indicates validation errors
            var errors = ParseValidationErrors(ex.StdErr);
            return ValidationResult.Failure(errors.ToArray());
        }
    }

    /// <summary>
    /// Generic CLI execution wrapper.
    /// </summary>
    public async Task<string> ExecuteCommand(string[] args, TimeSpan? timeout = null)
    {
        using (LogContext.PushProperty("CorrelationId", GetOrCreateCorrelationId()))
        {
            var binaryPath = await _binaryManager.GetHledgerBinaryPath();
            var commandTimeout = timeout ?? _defaultTimeout;

            _logger.LogDebug("Executing hledger command: {Binary} {Args}", binaryPath, string.Join(" ", args));

        var processStartInfo = new ProcessStartInfo
        {
            FileName = binaryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in args)
        {
            processStartInfo.ArgumentList.Add(arg);
        }

        // Set working directory to hledger file directory if -f argument is present
        var fileArgIndex = Array.IndexOf(args, "-f");
        if (fileArgIndex >= 0 && fileArgIndex + 1 < args.Length)
        {
            var hledgerFilePath = args[fileArgIndex + 1];
            var directory = Path.GetDirectoryName(hledgerFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                processStartInfo.WorkingDirectory = directory;
            }
        }

        using var process = Process.Start(processStartInfo);
        if (process == null)
        {
            throw new HledgerProcessException(
                "Failed to start hledger process", -1, "", "Process.Start returned null");
        }

        // Read output asynchronously
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        // Wait for process with timeout
        var timeoutTask = Task.Delay(commandTimeout);
        var processTask = process.WaitForExitAsync();

        var completedTask = await Task.WhenAny(processTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Ignore kill errors
            }

            throw new HledgerProcessException(
                $"hledger command timed out after {commandTimeout.TotalSeconds} seconds",
                -1, "", "Timeout");
        }

        var output = await outputTask;
        var error = await errorTask;

            _logger.LogDebug(
                "hledger command completed: ExitCode={ExitCode}, OutputLength={OutputLength}, ErrorLength={ErrorLength}",
                process.ExitCode, output.Length, error.Length);

            if (process.ExitCode != 0)
            {
                _logger.LogError(
                    "hledger command failed: {Command} {Args}\nExit Code: {ExitCode}\nStderr: {StdErr}",
                    binaryPath, string.Join(" ", args), process.ExitCode, error);

                throw new HledgerProcessException(
                    $"hledger command failed with exit code {process.ExitCode}",
                    process.ExitCode, output, error);
            }

            return output;
        }
    }

    private static List<string> ParseValidationErrors(string stderr)
    {
        if (string.IsNullOrWhiteSpace(stderr))
        {
            return new List<string> { "Unknown validation error" };
        }

        // Split by newlines and filter out empty lines
        return stderr
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
    }

    private string GetOrCreateCorrelationId()
    {
        // Try to get correlation ID from HTTP context
        var correlationId = _httpContextAccessor?.HttpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault();

        // If not in HTTP context, generate a new one
        return correlationId ?? Guid.NewGuid().ToString();
    }
}
