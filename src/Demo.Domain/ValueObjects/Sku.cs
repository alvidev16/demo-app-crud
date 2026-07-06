using Demo.Domain.Exceptions;

namespace Demo.Domain.ValueObjects;

/// <summary>
/// Stock Keeping Unit. A value object: immutable, compared by value, and it can
/// never hold an invalid state — construction goes through <see cref="Create"/>.
/// </summary>
public sealed record Sku
{
    public string Value { get; }

    private Sku(string value) => Value = value;

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException(nameof(Sku), "SKU is required.");

        return new Sku(value.Trim());
    }

    public override string ToString() => Value;
}
