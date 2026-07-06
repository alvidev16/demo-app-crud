using Demo.Domain.ValueObjects;

namespace Demo.Domain.Entities;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}

/// <summary>
/// Application user. The email is an <see cref="ValueObjects.Email"/> value object
/// (validated + normalized); passwords are never stored in plain text.
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = Roles.User;
    public DateTime CreatedAt { get; private set; }

    // Required by EF Core for materialization.
    private User() { }

    public static User Create(string email, string passwordHash, string role = Roles.User)
    {
        // Email.Create enforces format + normalization; throws ValidationException if invalid.
        var emailVo = Email.Create(email);

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new Exceptions.ValidationException(nameof(passwordHash), "Password hash is required.");

        return new User
        {
            Id = Guid.NewGuid(),
            Email = emailVo,
            PasswordHash = passwordHash,
            Role = string.IsNullOrWhiteSpace(role) ? Roles.User : role,
            CreatedAt = DateTime.UtcNow
        };
    }
}
