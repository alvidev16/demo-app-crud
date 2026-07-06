using Demo.Domain.Entities;

namespace Demo.TestSupport;

/// <summary>
/// Test Data Builder for <see cref="Product"/>. Starts from a valid default and
/// lets each test override only the fields it cares about, keeping arrange blocks
/// intention-revealing: <c>new ProductBuilder().WithSku("X").Build()</c>.
/// </summary>
public class ProductBuilder
{
    private string _name = "Sample Product";
    private string _sku = "SKU-0001";
    private decimal _price = 9.99m;
    private int _stock = 10;
    private string _category = "General";

    public ProductBuilder WithName(string name) { _name = name; return this; }
    public ProductBuilder WithSku(string sku) { _sku = sku; return this; }
    public ProductBuilder WithPrice(decimal price) { _price = price; return this; }
    public ProductBuilder WithStock(int stock) { _stock = stock; return this; }
    public ProductBuilder WithCategory(string category) { _category = category; return this; }

    /// <summary>Gives each product a unique SKU — handy when persisting several.</summary>
    public ProductBuilder WithUniqueSku() { _sku = $"SKU-{Guid.NewGuid():N}"; return this; }

    public Product Build() => Product.Create(_name, _sku, _price, _stock, _category);
}
