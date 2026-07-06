using Demo.DAL.Repositories;
using Demo.Domain.Entities;
using Demo.TestSupport;
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
        stored!.Email.Value.Should().Be("person@demo.com");
        stored.Role.Should().Be(Roles.Admin);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsMatchingUser()
    {
        var repo = NewRepository();
        await repo.AddAsync(User.Create("find@demo.com", "HASHED"));

        var found = await NewRepository().GetByEmailAsync("find@demo.com");

        found.Should().NotBeNull();
        found!.Email.Value.Should().Be("find@demo.com");
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
        await repo.AddAsync(new UserBuilder().WithEmail("dup@demo.com").Build());

        var duplicate = new UserBuilder().WithEmail("dup@demo.com").Build();
        var act = async () => await NewRepository().AddAsync(duplicate);

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
