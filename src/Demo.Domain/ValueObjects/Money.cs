using Demo.Domain.Exceptions;

namespace Demo.Domain.ValueObjects;

/// <summary>
/// A monetary amount for a catalog price. Value object with a positive invariant:
/// a product price must be greater than zero. (A general-purpose money type would
/// parameterize this rule; here it models the catalog domain specifically.)
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    public static Money Create(decimal amount)
    {
        if (amount <= 0)
            throw new ValidationException("Price", "Price must be greater than 0.");

        return new Money(decimal.Round(amount, 2));
    }

    public override string ToString() => Amount.ToString("0.00");
}
