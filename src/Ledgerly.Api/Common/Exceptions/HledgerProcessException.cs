namespace Ledgerly.Api.Common.Exceptions;

/// <summary>
/// Exception thrown when hledger process execution fails.
/// </summary>
public class HledgerProcessException : HledgerException
{
    public int ExitCode { get; }
    public string StdOut { get; }
    public string StdErr { get; }

    public HledgerProcessException(string message, int exitCode, string stdOut, string stdErr)
        : base(message)
    {
        ExitCode = exitCode;
        StdOut = stdOut;
        StdErr = stdErr;
    }

    public HledgerProcessException(string message, int exitCode, string stdOut, string stdErr, Exception innerException)
        : base(message, innerException)
    {
        ExitCode = exitCode;
        StdOut = stdOut;
        StdErr = stdErr;
    }
}
