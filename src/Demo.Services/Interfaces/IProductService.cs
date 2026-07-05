using Demo.Services.DTOs;

namespace Demo.Services.Interfaces;

/// <summary>Business operations for products (consumed by the API layer).</summary>
public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
