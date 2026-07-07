using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Tasks.Api.Auth;

/// <summary>JWT settings bound from the "Jwt" configuration section (never hard-coded — NFR-4).</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 120;
}

/// <summary>
/// Issues signed JWTs carrying the user id. Stands in for the host application's existing
/// authentication (the spec assumes a User model + auth already exist).
/// </summary>
public class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public string CreateToken(Guid userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
