namespace Ledgerly.Api.Common.Exceptions;

/// <summary>
/// Base exception for all hledger-related errors.
/// </summary>
public class HledgerException : Exception
{
    public HledgerException(string message) : base(message) { }
    public HledgerException(string message, Exception innerException) : base(message, innerException) { }
}
