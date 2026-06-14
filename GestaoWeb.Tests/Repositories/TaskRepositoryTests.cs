using GestaoWeb.Models.Domain;
using GestaoWeb.Repositories;
using GestaoWeb.Tests.Helpers;

namespace GestaoWeb.Tests.Repositories;

public class TaskRepositoryTests
{
    private static AppUser NewUser(string name, bool isManager = false) => new()
    {
        Id = Guid.NewGuid().ToString(),
        UserName = $"{name}@test.com",
        Email = $"{name}@test.com",
        FullName = name,
        MobilePhone = "(11) 99999-0000",
        Address = "Rua Teste, 1",
        BirthDate = new DateOnly(1990, 1, 1),
        IsManager = isManager
    };

    private static TaskItem NewTask(string description, string createdById, string assignedToId) => new()
    {
        Description = description,
        DueDate = DateTime.UtcNow.AddDays(3),
        CreatedById = createdById,
        AssignedToId = assignedToId,
        Status = WorkTaskStatus.Pending
    };

    [Fact]
    public async Task GetByManagerAsync_ReturnsOnlyTasksCreatedByManager()
    {
        var db = InMemoryFactory.CreateDbContext();
        var manager = NewUser("Manager", isManager: true);
        var other = NewUser("Other", isManager: true);
        var sub = NewUser("Sub");
        db.Users.AddRange(manager, other, sub);
        db.TaskItems.AddRange(
            NewTask("Task A", manager.Id, sub.Id),
            NewTask("Task B", manager.Id, sub.Id),
            NewTask("Task C", other.Id, sub.Id)
        );
        await db.SaveChangesAsync();

        var repo = new TaskRepository(db);
        var result = (await repo.GetByManagerAsync(manager.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(manager.Id, t.CreatedById));
    }

    [Fact]
    public async Task GetByAssigneeAsync_ReturnsOnlyTasksAssignedToUser()
    {
        var db = InMemoryFactory.CreateDbContext();
        var manager = NewUser("Manager", isManager: true);
        var sub1 = NewUser("Sub1");
        var sub2 = NewUser("Sub2");
        db.Users.AddRange(manager, sub1, sub2);
        db.TaskItems.AddRange(
            NewTask("For Sub1", manager.Id, sub1.Id),
            NewTask("For Sub2", manager.Id, sub2.Id)
        );
        await db.SaveChangesAsync();

        var repo = new TaskRepository(db);
        var result = (await repo.GetByAssigneeAsync(sub1.Id)).ToList();

        Assert.Single(result);
        Assert.Equal("For Sub1", result[0].Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTaskWithNavProperties()
    {
        var db = InMemoryFactory.CreateDbContext();
        var manager = NewUser("Manager", isManager: true);
        var sub = NewUser("Sub");
        db.Users.AddRange(manager, sub);
        var task = NewTask("My Task", manager.Id, sub.Id);
        db.TaskItems.Add(task);
        await db.SaveChangesAsync();

        var repo = new TaskRepository(db);
        var found = await repo.GetByIdAsync(task.Id);

        Assert.NotNull(found);
        Assert.Equal("My Task", found.Description);
        Assert.NotNull(found.CreatedBy);
        Assert.NotNull(found.AssignedTo);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var db = InMemoryFactory.CreateDbContext();
        var repo = new TaskRepository(db);

        var result = await repo.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_PersistsTask()
    {
        var db = InMemoryFactory.CreateDbContext();
        var manager = NewUser("Manager", isManager: true);
        var sub = NewUser("Sub");
        db.Users.AddRange(manager, sub);
        await db.SaveChangesAsync();

        var repo = new TaskRepository(db);
        var task = NewTask("New Task", manager.Id, sub.Id);
        var created = await repo.CreateAsync(task);

        Assert.True(created.Id > 0);
        Assert.Equal(1, db.TaskItems.Count());
    }

    [Fact]
    public async Task UpdateAsync_ChangesStatus()
    {
        var db = InMemoryFactory.CreateDbContext();
        var manager = NewUser("Manager", isManager: true);
        var sub = NewUser("Sub");
        db.Users.AddRange(manager, sub);
        var task = NewTask("Task", manager.Id, sub.Id);
        db.TaskItems.Add(task);
        await db.SaveChangesAsync();

        task.Status = WorkTaskStatus.InProgress;
        var repo = new TaskRepository(db);
        await repo.UpdateAsync(task);

        var updated = await db.TaskItems.FindAsync(task.Id);
        Assert.Equal(WorkTaskStatus.InProgress, updated!.Status);
    }
}
