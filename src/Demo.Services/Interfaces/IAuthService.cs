using Demo.Services.DTOs;

namespace Demo.Services.Interfaces;

/// <summary>Business operations for authentication.</summary>
public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default);
}
