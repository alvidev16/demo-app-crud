using Demo.DAL.Data;
using Demo.Domain.Entities;
using Demo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Demo.DAL.Repositories;

/// <summary>EF Core implementation of <see cref="IProductRepository"/>.</summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Products.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToListAsync(ct);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default) =>
        await _db.Products.FirstOrDefaultAsync(p => p.Sku == sku, ct);

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await _db.Products.AddAsync(product, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
    }
}
