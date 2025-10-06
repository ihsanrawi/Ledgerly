namespace Ledgerly.Api.Common.Exceptions;

/// <summary>
/// Exception thrown when hledger binary cannot be found or extracted.
/// </summary>
public class HledgerBinaryNotFoundException : HledgerException
{
    public HledgerBinaryNotFoundException(string message) : base(message) { }
    public HledgerBinaryNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
