using Demo.Domain.Entities;

namespace Demo.Services.Interfaces;

/// <summary>Abstraction over password hashing so the services are not tied to a specific library.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

/// <summary>Issues signed JWT access tokens for authenticated users.</summary>
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
