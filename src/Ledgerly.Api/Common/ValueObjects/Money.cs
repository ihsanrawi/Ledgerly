namespace Ledgerly.Api.Common.ValueObjects;

/// <summary>
/// Value object representing monetary amounts.
/// CRITICAL: Stores as INTEGER cents to avoid floating-point precision issues.
/// </summary>
public readonly record struct Money
{
    /// <summary>
    /// Amount in cents (integer to avoid floating-point precision errors).
    /// </summary>
    public long Cents { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> struct from cents.
    /// </summary>
    /// <param name="cents">Amount in cents.</param>
    public Money(long cents)
    {
        Cents = cents;
    }

    /// <summary>
    /// Creates a Money instance from a decimal amount.
    /// </summary>
    /// <param name="amount">Decimal amount.</param>
    /// <returns>Money instance.</returns>
    public static Money FromDecimal(decimal amount)
    {
        return new Money((long)(amount * 100));
    }

    /// <summary>
    /// Converts the Money instance to a decimal amount.
    /// </summary>
    /// <returns>Decimal representation.</returns>
    public decimal ToDecimal()
    {
        return Cents / 100m;
    }

    /// <summary>
    /// Adds two Money instances.
    /// </summary>
    public static Money operator +(Money a, Money b)
    {
        return new Money(a.Cents + b.Cents);
    }

    /// <summary>
    /// Subtracts two Money instances.
    /// </summary>
    public static Money operator -(Money a, Money b)
    {
        return new Money(a.Cents - b.Cents);
    }

    /// <summary>
    /// Multiplies Money by a scalar.
    /// </summary>
    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money((long)(money.Cents * multiplier));
    }

    public override string ToString()
    {
        return $"${ToDecimal():N2}";
    }
}
