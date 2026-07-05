using Demo.DAL.Data;
using Demo.Domain.Entities;
using Demo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Demo.DAL.Repositories;

/// <summary>EF Core implementation of <see cref="IUserRepository"/>.</summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
    }
}
