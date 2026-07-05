using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Demo.DAL.Data;
using Demo.Services.DTOs;
using FluentAssertions;

namespace Demo.Api.Tests;

public class ProductsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProductsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto(DbSeeder.DemoAdminEmail, DbSeeder.DemoAdminPassword));
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthResultDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    [Fact]
    public async Task GetProducts_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProducts_WithToken_ReturnsSeededProducts()
    {
        var client = await CreateAuthenticatedClientAsync();

        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/products");

        products.Should().NotBeNull();
        products!.Count.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task CreateProduct_ThenGetById_RoundTrips()
    {
        var client = await CreateAuthenticatedClientAsync();
        var dto = new CreateProductDto("Test Widget", $"SKU-{Guid.NewGuid():N}", 9.99m, 7, "Gadgets");

        var create = await client.PostAsJsonAsync("/api/products", dto);
        create.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await create.Content.ReadFromJsonAsync<ProductDto>();
        created!.Id.Should().NotBeEmpty();

        var fetched = await client.GetFromJsonAsync<ProductDto>($"/api/products/{created.Id}");
        fetched!.Name.Should().Be("Test Widget");
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_Returns400()
    {
        var client = await CreateAuthenticatedClientAsync();
        var dto = new CreateProductDto("", "SKU-X", -5m, -1, "");

        var response = await client.PostAsJsonAsync("/api/products", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_Returns409()
    {
        var client = await CreateAuthenticatedClientAsync();
        var sku = $"SKU-{Guid.NewGuid():N}";
        await client.PostAsJsonAsync("/api/products", new CreateProductDto("First", sku, 10m, 1, "Cat"));

        var duplicate = await client.PostAsJsonAsync("/api/products", new CreateProductDto("Second", sku, 20m, 2, "Cat"));

        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateProduct_ChangesFields()
    {
        var client = await CreateAuthenticatedClientAsync();
        var create = await client.PostAsJsonAsync("/api/products",
            new CreateProductDto("Before", $"SKU-{Guid.NewGuid():N}", 10m, 1, "Cat"));
        var created = await create.Content.ReadFromJsonAsync<ProductDto>();

        var update = await client.PutAsJsonAsync($"/api/products/{created!.Id}",
            new UpdateProductDto("After", created.Sku, 55m, 9, "NewCat"));
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await update.Content.ReadFromJsonAsync<ProductDto>();
        updated!.Name.Should().Be("After");
        updated.Price.Should().Be(55m);
    }

    [Fact]
    public async Task DeleteProduct_ThenGet_Returns404()
    {
        var client = await CreateAuthenticatedClientAsync();
        var create = await client.PostAsJsonAsync("/api/products",
            new CreateProductDto("ToDelete", $"SKU-{Guid.NewGuid():N}", 10m, 1, "Cat"));
        var created = await create.Content.ReadFromJsonAsync<ProductDto>();

        var delete = await client.DeleteAsync($"/api/products/{created!.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await client.GetAsync($"/api/products/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
