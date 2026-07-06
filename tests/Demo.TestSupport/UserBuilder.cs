using Demo.Domain.Entities;

namespace Demo.TestSupport;

/// <summary>Test Data Builder for <see cref="User"/>.</summary>
public class UserBuilder
{
    private string _email = "user@demo.com";
    private string _passwordHash = "hashed-password";
    private string _role = Roles.User;

    public UserBuilder WithEmail(string email) { _email = email; return this; }
    public UserBuilder WithPasswordHash(string hash) { _passwordHash = hash; return this; }
    public UserBuilder AsAdmin() { _role = Roles.Admin; return this; }
    public UserBuilder WithUniqueEmail() { _email = $"user-{Guid.NewGuid():N}@demo.com"; return this; }

    public User Build() => User.Create(_email, _passwordHash, _role);
}
