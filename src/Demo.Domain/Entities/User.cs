using Demo.Domain.Exceptions;

namespace Demo.Domain.Entities;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}

/// <summary>
/// Application user. Passwords are never stored in plain text: the entity only
/// ever holds an already-computed hash.
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = Roles.User;
    public DateTime CreatedAt { get; private set; }

    // Required by EF Core for materialization.
    private User() { }

    public static User Create(string email, string passwordHash, string role = Roles.User)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            errors[nameof(Email)] = "A valid email is required.";

        if (string.IsNullOrWhiteSpace(passwordHash))
            errors[nameof(passwordHash)] = "Password hash is required.";

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = string.IsNullOrWhiteSpace(role) ? Roles.User : role,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }
}
