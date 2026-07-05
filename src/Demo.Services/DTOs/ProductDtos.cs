using Demo.Domain.Entities;

namespace Demo.Services.DTOs;

/// <summary>Product data returned to clients.</summary>
public record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    decimal Price,
    int Stock,
    string Category,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static ProductDto FromEntity(Product p) =>
        new(p.Id, p.Name, p.Sku, p.Price, p.Stock, p.Category, p.CreatedAt, p.UpdatedAt);
}

/// <summary>Payload to create a product.</summary>
public record CreateProductDto(string Name, string Sku, decimal Price, int Stock, string Category);

/// <summary>Payload to update a product.</summary>
public record UpdateProductDto(string Name, string Sku, decimal Price, int Stock, string Category);
