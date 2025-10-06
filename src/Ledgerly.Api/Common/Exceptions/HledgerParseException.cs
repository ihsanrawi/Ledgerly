namespace Ledgerly.Api.Common.Exceptions;

/// <summary>
/// Exception thrown when hledger output cannot be parsed.
/// </summary>
public class HledgerParseException : HledgerException
{
    public string RawOutput { get; }

    public HledgerParseException(string message, string rawOutput) : base(message)
    {
        RawOutput = rawOutput;
    }

    public HledgerParseException(string message, string rawOutput, Exception innerException)
        : base(message, innerException)
    {
        RawOutput = rawOutput;
    }
}
