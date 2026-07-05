using Demo.Domain.Entities;
using Demo.Domain.Exceptions;
using Demo.Domain.Interfaces;
using Demo.Services.DTOs;
using Demo.Services.Interfaces;

namespace Demo.Services;

/// <summary>
/// Authentication business logic: registration with unique-email and password
/// rules, and login that verifies credentials and issues a JWT.
/// </summary>
public class AuthService : IAuthService
{
    private const int MinPasswordLength = 6;

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(IUserRepository users, IPasswordHasher hasher, ITokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        ValidateRegistration(dto);

        var email = dto.Email.Trim().ToLowerInvariant();
        var existing = await _users.GetByEmailAsync(email, ct);
        if (existing is not null)
            throw new ConflictException($"A user with email '{email}' already exists.");

        var hash = _hasher.Hash(dto.Password);
        var user = User.Create(email, hash); // defaults to Role = User

        await _users.AddAsync(user, ct);
        return UserDto.FromEntity(user);
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email, ct)
            ?? throw new InvalidCredentialsException();

        if (!_hasher.Verify(dto.Password ?? string.Empty, user.PasswordHash))
            throw new InvalidCredentialsException();

        var (token, expiresAt) = _tokens.CreateToken(user);
        return new AuthResultDto(token, expiresAt, UserDto.FromEntity(user));
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(User), id);
        return UserDto.FromEntity(user);
    }

    private static void ValidateRegistration(RegisterDto dto)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
            errors[nameof(dto.Email)] = "A valid email is required.";

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < MinPasswordLength)
            errors[nameof(dto.Password)] = $"Password must be at least {MinPasswordLength} characters.";

        if (errors.Count > 0)
            throw new ValidationException(errors);
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
