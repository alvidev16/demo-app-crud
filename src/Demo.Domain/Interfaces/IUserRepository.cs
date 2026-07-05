using Demo.Domain.Entities;

namespace Demo.Domain.Interfaces;

/// <summary>Data-access contract for users.</summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
