using Demo.Domain.Exceptions;
using Demo.Domain.ValueObjects;

namespace Demo.Domain.Entities;

/// <summary>
/// Product aggregate. Business invariants are enforced here (and inside the value
/// objects it holds) so the entity can never exist in an invalid state.
/// </summary>
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Sku Sku { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public int Stock { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Required by EF Core for materialization.
    private Product() { }

    public static Product Create(string name, string sku, decimal price, int stock, string category)
    {
        var (skuVo, priceVo) = Validate(name, sku, price, stock, category);
        var now = DateTime.UtcNow;
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Sku = skuVo,
            Price = priceVo,
            Stock = stock,
            Category = category.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string name, string sku, decimal price, int stock, string category)
    {
        var (skuVo, priceVo) = Validate(name, sku, price, stock, category);
        Name = name.Trim();
        Sku = skuVo;
        Price = priceVo;
        Stock = stock;
        Category = category.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates all fields, aggregating errors across primitive rules and the
    /// value objects (SKU, price) so the caller gets every problem at once.
    /// </summary>
    private static (Sku, Money) Validate(string name, string sku, decimal price, int stock, string category)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 3 || name.Trim().Length > 100)
            errors[nameof(Name)] = "Name is required and must be between 3 and 100 characters.";

        if (stock < 0)
            errors[nameof(Stock)] = "Stock cannot be negative.";

        if (string.IsNullOrWhiteSpace(category))
            errors[nameof(Category)] = "Category is required.";

        Sku? skuVo = TryBuild(() => Sku.Create(sku), errors);
        Money? priceVo = TryBuild(() => Money.Create(price), errors);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return (skuVo!, priceVo!);
    }

    private static T? TryBuild<T>(Func<T> factory, Dictionary<string, string> errors) where T : class
    {
        try
        {
            return factory();
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                errors[error.Key] = error.Value;
            return null;
        }
    }
}
