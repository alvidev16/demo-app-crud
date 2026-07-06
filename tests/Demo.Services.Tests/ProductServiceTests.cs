using Demo.Domain.Entities;
using Demo.Domain.Exceptions;
using Demo.Domain.Interfaces;
using Demo.Services.DTOs;
using FluentAssertions;
using Moq;

namespace Demo.Services.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repo.Object);
    }

    private static CreateProductDto ValidCreate(string sku = "SKU-001") =>
        new("Wireless Mouse", sku, 19.99m, 50, "Peripherals");

    // ---------- Create ----------

    [Fact]
    public async Task CreateAsync_WithValidData_PersistsAndReturnsDto()
    {
        _repo.Setup(r => r.GetBySkuAsync("SKU-001", It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var result = await _sut.CreateAsync(ValidCreate());

        result.Name.Should().Be("Wireless Mouse");
        result.Sku.Should().Be("SKU-001");
        result.Price.Should().Be(19.99m);
        result.Id.Should().NotBeEmpty();
        _repo.Verify(r => r.AddAsync(It.Is<Product>(p => p.Sku.Value == "SKU-001"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateSku_ThrowsConflict()
    {
        _repo.Setup(r => r.GetBySkuAsync("SKU-001", It.IsAny<CancellationToken>()))
             .ReturnsAsync(Product.Create("Existing", "SKU-001", 5m, 1, "X"));

        var act = () => _sut.CreateAsync(ValidCreate());

        await act.Should().ThrowAsync<ConflictException>();
        _repo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", "SKU-1", 10, 5, "Cat")]        // empty name
    [InlineData("ab", "SKU-1", 10, 5, "Cat")]      // name too short
    [InlineData("Valid", "", 10, 5, "Cat")]        // empty sku
    [InlineData("Valid", "SKU-1", 0, 5, "Cat")]    // price not > 0
    [InlineData("Valid", "SKU-1", 10, -1, "Cat")]  // negative stock
    [InlineData("Valid", "SKU-1", 10, 5, "")]      // empty category
    public async Task CreateAsync_WithInvalidData_ThrowsValidation(
        string name, string sku, decimal price, int stock, string category)
    {
        _repo.Setup(r => r.GetBySkuAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var act = () => _sut.CreateAsync(new CreateProductDto(name, sku, price, stock, category));

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ---------- Read ----------

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsDto()
    {
        var product = Product.Create("Keyboard", "SKU-KB", 49.90m, 10, "Peripherals");
        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(product.Id);

        result.Sku.Should().Be("SKU-KB");
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Product>
             {
                 Product.Create("Alpha", "SKU-A", 1m, 1, "Cat"),
                 Product.Create("Bravo", "SKU-B", 2m, 2, "Cat")
             });

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    // ---------- Update ----------

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesFields()
    {
        var product = Product.Create("Old", "SKU-1", 10m, 5, "Cat");
        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(product);
        _repo.Setup(r => r.GetBySkuAsync("SKU-1", It.IsAny<CancellationToken>()))
             .ReturnsAsync(product);

        var result = await _sut.UpdateAsync(product.Id, new UpdateProductDto("New Name", "SKU-1", 99m, 3, "NewCat"));

        result.Name.Should().Be("New Name");
        result.Price.Should().Be(99m);
        _repo.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), new UpdateProductDto("N", "S", 1m, 1, "C"));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ChangingToExistingSku_ThrowsConflict()
    {
        var target = Product.Create("Target", "SKU-1", 10m, 5, "Cat");
        var other = Product.Create("Other", "SKU-2", 10m, 5, "Cat");
        _repo.Setup(r => r.GetByIdAsync(target.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(target);
        _repo.Setup(r => r.GetBySkuAsync("SKU-2", It.IsAny<CancellationToken>()))
             .ReturnsAsync(other);

        var act = () => _sut.UpdateAsync(target.Id, new UpdateProductDto("Target", "SKU-2", 10m, 5, "Cat"));

        await act.Should().ThrowAsync<ConflictException>();
    }

    // ---------- Delete ----------

    [Fact]
    public async Task DeleteAsync_WhenExists_CallsRepository()
    {
        var product = Product.Create("Alpha", "SKU-A", 1m, 1, "Cat");
        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(product);

        await _sut.DeleteAsync(product.Id);

        _repo.Verify(r => r.DeleteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
