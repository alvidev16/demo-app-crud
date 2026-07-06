using Demo.DAL.Repositories;
using Demo.Domain.Entities;
using Demo.TestSupport;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Demo.DAL.Tests;

public class ProductRepositoryTests : SqliteTestBase
{
    private ProductRepository NewRepository() => new(NewContext());

    [Fact]
    public async Task AddAsync_PersistsProduct()
    {
        var repo = NewRepository();
        var product = Product.Create("Keyboard", "SKU-KB-1", 49.90m, 10, "Peripherals");

        await repo.AddAsync(product);

        var stored = await NewRepository().GetByIdAsync(product.Id);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be("Keyboard");
        stored.Price.Amount.Should().Be(49.90m);
    }

    [Fact]
    public async Task GetBySkuAsync_ReturnsMatchingProduct()
    {
        var repo = NewRepository();
        await repo.AddAsync(Product.Create("Mouse", "SKU-MO-1", 19.99m, 5, "Peripherals"));

        var found = await NewRepository().GetBySkuAsync("SKU-MO-1");

        found.Should().NotBeNull();
        found!.Sku.Value.Should().Be("SKU-MO-1");
    }

    [Fact]
    public async Task GetBySkuAsync_WhenMissing_ReturnsNull()
    {
        var found = await NewRepository().GetBySkuAsync("does-not-exist");

        found.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsNewestFirst()
    {
        var repo = NewRepository();
        await repo.AddAsync(new ProductBuilder().WithName("Older").WithSku("SKU-A").Build());
        await Task.Delay(5); // ensure a distinct CreatedAt
        await repo.AddAsync(new ProductBuilder().WithName("Newer").WithSku("SKU-B").Build());

        var all = await NewRepository().GetAllAsync();

        all.Should().HaveCount(2);
        all[0].Sku.Value.Should().Be("SKU-B"); // ordered by CreatedAt desc
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var repo = NewRepository();
        var product = Product.Create("Before", "SKU-UP", 10m, 1, "Cat");
        await repo.AddAsync(product);

        var toUpdate = await NewRepository().GetByIdAsync(product.Id);
        toUpdate!.Update("After", "SKU-UP", 99m, 3, "NewCat");
        await NewRepository().UpdateAsync(toUpdate);

        var reloaded = await NewRepository().GetByIdAsync(product.Id);
        reloaded!.Name.Should().Be("After");
        reloaded.Price.Amount.Should().Be(99m);
        reloaded.Category.Should().Be("NewCat");
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var repo = NewRepository();
        var product = Product.Create("ToDelete", "SKU-DEL", 10m, 1, "Cat");
        await repo.AddAsync(product);

        var toDelete = await NewRepository().GetByIdAsync(product.Id);
        await NewRepository().DeleteAsync(toDelete!);

        var reloaded = await NewRepository().GetByIdAsync(product.Id);
        reloaded.Should().BeNull();
    }

    [Fact]
    public async Task UniqueIndex_RejectsDuplicateSku()
    {
        var repo = NewRepository();
        await repo.AddAsync(new ProductBuilder().WithSku("SKU-DUP").Build());

        var duplicate = new ProductBuilder().WithSku("SKU-DUP").Build();
        var act = async () => await NewRepository().AddAsync(duplicate);

        // The database-level unique index must reject the duplicate SKU.
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
