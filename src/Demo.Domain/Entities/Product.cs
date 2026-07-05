using Demo.Domain.Exceptions;

namespace Demo.Domain.Entities;

/// <summary>
/// Product aggregate. Business invariants are enforced here so the entity can
/// never exist in an invalid state, regardless of which layer creates it.
/// </summary>
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Required by EF Core for materialization.
    private Product() { }

    public static Product Create(string name, string sku, decimal price, int stock, string category)
    {
        Validate(name, sku, price, stock, category);
        var now = DateTime.UtcNow;
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Sku = sku.Trim(),
            Price = price,
            Stock = stock,
            Category = category.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string name, string sku, decimal price, int stock, string category)
    {
        Validate(name, sku, price, stock, category);
        Name = name.Trim();
        Sku = sku.Trim();
        Price = price;
        Stock = stock;
        Category = category.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string name, string sku, decimal price, int stock, string category)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 3 || name.Trim().Length > 100)
            errors[nameof(Name)] = "Name is required and must be between 3 and 100 characters.";

        if (string.IsNullOrWhiteSpace(sku))
            errors[nameof(Sku)] = "SKU is required.";

        if (price <= 0)
            errors[nameof(Price)] = "Price must be greater than 0.";

        if (stock < 0)
            errors[nameof(Stock)] = "Stock cannot be negative.";

        if (string.IsNullOrWhiteSpace(category))
            errors[nameof(Category)] = "Category is required.";

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}
