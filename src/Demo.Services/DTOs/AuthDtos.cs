using Demo.Domain.Entities;

namespace Demo.Services.DTOs;

/// <summary>Registration payload.</summary>
public record RegisterDto(string Email, string Password);

/// <summary>Login payload.</summary>
public record LoginDto(string Email, string Password);

/// <summary>Public view of a user (never exposes the password hash).</summary>
public record UserDto(Guid Id, string Email, string Role, DateTime CreatedAt)
{
    public static UserDto FromEntity(User u) => new(u.Id, u.Email.Value, u.Role, u.CreatedAt);
}

/// <summary>Result of a successful login: the JWT plus basic user info.</summary>
public record AuthResultDto(string Token, DateTime ExpiresAt, UserDto User);
