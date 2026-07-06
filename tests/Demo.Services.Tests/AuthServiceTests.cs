using Demo.Domain.Entities;
using Demo.Domain.Exceptions;
using Demo.Domain.Interfaces;
using Demo.Services.DTOs;
using Demo.Services.Interfaces;
using FluentAssertions;
using Moq;

namespace Demo.Services.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_users.Object, _hasher.Object, _tokens.Object);
    }

    // ---------- Register ----------

    [Fact]
    public async Task RegisterAsync_WithNewEmail_HashesPasswordAndPersists()
    {
        _users.Setup(r => r.GetByEmailAsync("new@demo.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash("Secret123")).Returns("HASHED");

        var result = await _sut.RegisterAsync(new RegisterDto("new@demo.com", "Secret123"));

        result.Email.Should().Be("new@demo.com");
        result.Role.Should().Be(Roles.User);
        _users.Verify(r => r.AddAsync(
            It.Is<User>(u => u.PasswordHash == "HASHED" && u.Email.Value == "new@demo.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsConflict()
    {
        _users.Setup(r => r.GetByEmailAsync("taken@demo.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(User.Create("taken@demo.com", "HASH"));

        var act = () => _sut.RegisterAsync(new RegisterDto("taken@demo.com", "Secret123"));

        await act.Should().ThrowAsync<ConflictException>();
        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("not-an-email", "Secret123")]
    [InlineData("valid@demo.com", "123")]        // password too short (< 6)
    [InlineData("", "Secret123")]
    public async Task RegisterAsync_WithInvalidInput_ThrowsValidation(string email, string password)
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("HASHED");

        var act = () => _sut.RegisterAsync(new RegisterDto(email, password));

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ---------- Login ----------

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = User.Create("user@demo.com", "STORED_HASH");
        _users.Setup(r => r.GetByEmailAsync("user@demo.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Secret123", "STORED_HASH")).Returns(true);
        _tokens.Setup(t => t.CreateToken(user))
               .Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));

        var result = await _sut.LoginAsync(new LoginDto("user@demo.com", "Secret123"));

        result.Token.Should().Be("jwt-token");
        result.User.Email.Should().Be("user@demo.com");
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsInvalidCredentials()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(new LoginDto("ghost@demo.com", "whatever"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsInvalidCredentials()
    {
        var user = User.Create("user@demo.com", "STORED_HASH");
        _users.Setup(r => r.GetByEmailAsync("user@demo.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("wrong", "STORED_HASH")).Returns(false);

        var act = () => _sut.LoginAsync(new LoginDto("user@demo.com", "wrong"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
        _tokens.Verify(t => t.CreateToken(It.IsAny<User>()), Times.Never);
    }

    // ---------- GetById ----------

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        _users.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
