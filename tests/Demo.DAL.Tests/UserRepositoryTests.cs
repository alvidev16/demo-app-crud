using Demo.DAL.Repositories;
using Demo.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Demo.DAL.Tests;

public class UserRepositoryTests : SqliteTestBase
{
    private UserRepository NewRepository() => new(NewContext());

    [Fact]
    public async Task AddAsync_PersistsUser()
    {
        var repo = NewRepository();
        var user = User.Create("person@demo.com", "HASHED", Roles.Admin);

        await repo.AddAsync(user);

        var stored = await NewRepository().GetByIdAsync(user.Id);
        stored.Should().NotBeNull();
        stored!.Email.Should().Be("person@demo.com");
        stored.Role.Should().Be(Roles.Admin);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsMatchingUser()
    {
        var repo = NewRepository();
        await repo.AddAsync(User.Create("find@demo.com", "HASHED"));

        var found = await NewRepository().GetByEmailAsync("find@demo.com");

        found.Should().NotBeNull();
        found!.Email.Should().Be("find@demo.com");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenMissing_ReturnsNull()
    {
        var found = await NewRepository().GetByEmailAsync("nobody@demo.com");

        found.Should().BeNull();
    }

    [Fact]
    public async Task UniqueIndex_RejectsDuplicateEmail()
    {
        var repo = NewRepository();
        await repo.AddAsync(User.Create("dup@demo.com", "HASH1"));

        var duplicate = User.Create("dup@demo.com", "HASH2");
        var act = async () => await NewRepository().AddAsync(duplicate);

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
