namespace Ledgerly.Api.Common.Exceptions;

/// <summary>
/// Exception thrown when hledger validation fails.
/// </summary>
public class HledgerValidationException : HledgerException
{
    /// <summary>
    /// Gets the exact error output from hledger CLI.
    /// </summary>
    public string HledgerErrorOutput { get; }

    /// <summary>
    /// Gets the file path that failed validation.
    /// </summary>
    public string FilePath { get; }

    public HledgerValidationException(string message, string hledgerErrorOutput, string filePath)
        : base(message)
    {
        HledgerErrorOutput = hledgerErrorOutput;
        FilePath = filePath;
    }

    public HledgerValidationException(string message) : base(message)
    {
        HledgerErrorOutput = string.Empty;
        FilePath = string.Empty;
    }

    public HledgerValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        HledgerErrorOutput = string.Empty;
        FilePath = string.Empty;
    }
}
