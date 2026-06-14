using GestaoWeb.Models.Domain;
using GestaoWeb.Repositories;
using GestaoWeb.Tests.Helpers;

namespace GestaoWeb.Tests.Repositories;

public class UserRepositoryTests
{
    private static AppUser NewUser(string fullName, string email, bool isManager = false) => new()
    {
        UserName = email,
        Email = email,
        FullName = fullName,
        MobilePhone = "(11) 99999-0000",
        Address = "Rua Teste, 1",
        BirthDate = new DateOnly(1990, 1, 1),
        IsManager = isManager
    };

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers_OrderedByName()
    {
        var (_, um) = InMemoryFactory.CreateUserManager();
        await um.CreateAsync(NewUser("Zara", "zara@test.com"), "Senha123");
        await um.CreateAsync(NewUser("Alice", "alice@test.com"), "Senha123");

        var repo = new UserRepository(um);
        var result = (await repo.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0].FullName);
        Assert.Equal("Zara", result[1].FullName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectUser()
    {
        var (_, um) = InMemoryFactory.CreateUserManager();
        var user = NewUser("Bob", "bob@test.com");
        await um.CreateAsync(user, "Senha123");

        var repo = new UserRepository(um);
        var found = await repo.GetByIdAsync(user.Id);

        Assert.NotNull(found);
        Assert.Equal("bob@test.com", found.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var (_, um) = InMemoryFactory.CreateUserManager();
        var repo = new UserRepository(um);

        var result = await repo.GetByIdAsync(Guid.NewGuid().ToString());

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_PersistsUser()
    {
        var (_, um) = InMemoryFactory.CreateUserManager();
        var repo = new UserRepository(um);
        var user = NewUser("Carol", "carol@test.com");

        var result = await repo.CreateAsync(user, "Senha123");

        Assert.True(result.Succeeded);
        Assert.NotNull(await um.FindByEmailAsync("carol@test.com"));
    }

    [Fact]
    public async Task UpdateAsync_SavesChanges()
    {
        var (_, um) = InMemoryFactory.CreateUserManager();
        var user = NewUser("Dave", "dave@test.com");
        await um.CreateAsync(user, "Senha123");

        user.FullName = "Dave Updated";
        var repo = new UserRepository(um);
        var result = await repo.UpdateAsync(user);

        Assert.True(result.Succeeded);
        var updated = await um.FindByIdAsync(user.Id);
        Assert.Equal("Dave Updated", updated!.FullName);
    }

    [Fact]
    public async Task ChangePasswordAsync_UpdatesPassword()
    {
        var (_, um) = InMemoryFactory.CreateUserManager();
        var user = NewUser("Eve", "eve@test.com");
        await um.CreateAsync(user, "Senha123");

        var repo = new UserRepository(um);
        var result = await repo.ChangePasswordAsync(user, "NovaSenh4");

        Assert.True(result.Succeeded);
        var checkResult = await um.CheckPasswordAsync(user, "NovaSenh4");
        Assert.True(checkResult);
    }
}
