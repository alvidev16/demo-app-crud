using Demo.Domain.Entities;

namespace Demo.Domain.Interfaces;

/// <summary>
/// Data-access contract for products. Defined in the Domain so business logic
/// depends on an abstraction, not on the DAL implementation (EF Core).
/// </summary>
public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Product product, CancellationToken ct = default);
}
