using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Demo.DAL.Data;
using Demo.Services.DTOs;
using FluentAssertions;

namespace Demo.Api.Tests;

public class AuthApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PublicEndpoint_IsAccessible_WithoutToken()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/public");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto(DbSeeder.DemoAdminEmail, DbSeeder.DemoAdminPassword));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
        auth.User.Email.Should().Be(DbSeeder.DemoAdminEmail);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto(DbSeeder.DemoAdminEmail, "wrong-password"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_ThenLogin_Succeeds()
    {
        var client = _factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@demo.com";

        var register = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto(email, "Password123"));
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto(email, "Password123"));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithExistingEmail_Returns409()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto(DbSeeder.DemoAdminEmail, "Password123"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Me_WithToken_ReturnsCurrentUser()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto(DbSeeder.DemoAdminEmail, DbSeeder.DemoAdminPassword));
        var auth = await login.Content.ReadFromJsonAsync<AuthResultDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me");

        me!.Email.Should().Be(DbSeeder.DemoAdminEmail);
    }
}
