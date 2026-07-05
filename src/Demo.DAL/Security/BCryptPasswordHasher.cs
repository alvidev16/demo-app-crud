using Demo.Services.Interfaces;

namespace Demo.DAL.Security;

/// <summary>BCrypt-based implementation of <see cref="IPasswordHasher"/>.</summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
