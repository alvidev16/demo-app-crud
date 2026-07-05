using Demo.Domain.Entities;
using Demo.Domain.Exceptions;
using Demo.Domain.Interfaces;
using Demo.Services.DTOs;
using Demo.Services.Interfaces;

namespace Demo.Services;

/// <summary>
/// Business logic for products: orchestrates validation, uniqueness rules
/// and persistence through the repository abstraction.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct = default)
    {
        var products = await _repository.GetAllAsync(ct);
        return products.Select(ProductDto.FromEntity).ToList();
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Product), id);
        return ProductDto.FromEntity(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        await EnsureSkuIsAvailableAsync(dto.Sku, excludingId: null, ct);

        // Entity factory enforces field-level invariants (throws ValidationException).
        var product = Product.Create(dto.Name, dto.Sku, dto.Price, dto.Stock, dto.Category);

        await _repository.AddAsync(product, ct);
        return ProductDto.FromEntity(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Product), id);

        await EnsureSkuIsAvailableAsync(dto.Sku, excludingId: id, ct);

        product.Update(dto.Name, dto.Sku, dto.Price, dto.Stock, dto.Category);

        await _repository.UpdateAsync(product, ct);
        return ProductDto.FromEntity(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Product), id);

        await _repository.DeleteAsync(product, ct);
    }

    /// <summary>Business rule: SKU must be unique across products.</summary>
    private async Task EnsureSkuIsAvailableAsync(string sku, Guid? excludingId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return; // field-level validation handles empties; nothing to check for uniqueness.

        var existing = await _repository.GetBySkuAsync(sku.Trim(), ct);
        if (existing is not null && existing.Id != excludingId)
            throw new ConflictException($"A product with SKU '{sku.Trim()}' already exists.");
    }
}
