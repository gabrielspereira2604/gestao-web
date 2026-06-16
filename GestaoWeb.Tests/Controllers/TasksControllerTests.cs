using System.Security.Claims;
using GestaoWeb.Controllers;
using GestaoWeb.Models.Domain;
using GestaoWeb.Models.ViewModels.Tasks;
using GestaoWeb.Repositories;
using GestaoWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GestaoWeb.Tests.Controllers;

public class TasksControllerTests
{
    private static Mock<UserManager<AppUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    private static TasksController CreateController(
        AppUser currentUser,
        Mock<UserManager<AppUser>>? umMock = null,
        Mock<ITaskRepository>? taskMock = null,
        Mock<IUserRepository>? userMock = null,
        Mock<IEmailService>? emailMock = null)
    {
        var um = umMock ?? CreateUserManagerMock();
        um.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
          .ReturnsAsync(currentUser);

        var controller = new TasksController(
            taskMock?.Object ?? new Mock<ITaskRepository>().Object,
            userMock?.Object ?? new Mock<IUserRepository>().Object,
            um.Object,
            emailMock?.Object ?? new Mock<IEmailService>().Object,
            NullLogger<TasksController>.Instance
        );

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, currentUser.Id) };
        var identity = new ClaimsIdentity(claims, "Test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        controller.TempData = new TempDataDictionary(
            controller.ControllerContext.HttpContext,
            Mock.Of<ITempDataProvider>());

        return controller;
    }

    private static AppUser Manager() => new()
    {
        Id = Guid.NewGuid().ToString(),
        UserName = "manager@test.com",
        Email = "manager@test.com",
        FullName = "Gestor",
        MobilePhone = "11999990000",
        Address = "Rua",
        BirthDate = new DateOnly(1990, 1, 1),
        IsManager = true
    };

    private static AppUser Collaborator(string? id = null) => new()
    {
        Id = id ?? Guid.NewGuid().ToString(),
        UserName = "sub@test.com",
        Email = "sub@test.com",
        FullName = "Colaborador",
        MobilePhone = "11999990000",
        Address = "Rua",
        BirthDate = new DateOnly(1995, 1, 1),
        IsManager = false
    };

    [Fact]
    public async Task Create_Get_NonManager_RedirectsToHome()
    {
        var controller = CreateController(Collaborator());

        var result = await controller.Create() as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Home", result.ControllerName);
    }

    [Fact]
    public async Task Create_Get_Manager_ReturnsView()
    {
        var userMock = new Mock<IUserRepository>();
        userMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<AppUser> { Collaborator() });

        var controller = CreateController(Manager(), userMock: userMock);

        var result = await controller.Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_NonManager_RedirectsToHome()
    {
        var controller = CreateController(Collaborator());
        var model = new CreateTaskViewModel
        {
            Description = "Tarefa",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            AssignedToId = Guid.NewGuid().ToString()
        };

        var result = await controller.Create(model) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Home", result.ControllerName);
    }

    [Fact]
    public async Task Edit_Post_NonOwnerCollaborator_RedirectsToIndex()
    {
        var sub = Collaborator();
        var otherId = Guid.NewGuid().ToString();

        var task = new TaskItem
        {
            Id = 1,
            Description = "Tarefa",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Status = WorkTaskStatus.Pending,
            AssignedToId = otherId,
            CreatedById = Guid.NewGuid().ToString(),
            CreatedBy = Manager(),
            AssignedTo = Collaborator(otherId)
        };

        var taskMock = new Mock<ITaskRepository>();
        taskMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

        var controller = CreateController(sub, taskMock: taskMock);
        var model = new EditTaskStatusViewModel { Id = 1, Status = WorkTaskStatus.InProgress };

        var result = await controller.Edit(model) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Null(result.ControllerName);
    }

    [Fact]
    public async Task Edit_Post_ValidCollaboratorOwner_UpdatesStatus()
    {
        var sub = Collaborator();
        var manager = Manager();

        var task = new TaskItem
        {
            Id = 1,
            Description = "Tarefa",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Status = WorkTaskStatus.Pending,
            AssignedToId = sub.Id,
            CreatedById = manager.Id,
            CreatedBy = manager,
            AssignedTo = sub
        };

        var taskMock = new Mock<ITaskRepository>();
        taskMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

        var controller = CreateController(sub, taskMock: taskMock);
        var model = new EditTaskStatusViewModel { Id = 1, Status = WorkTaskStatus.InProgress };

        var result = await controller.Edit(model) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        taskMock.Verify(r => r.UpdateAsync(It.Is<TaskItem>(t => t.Status == WorkTaskStatus.InProgress)), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_CollaboratorCannotRegress_ReturnsViewWithError()
    {
        var sub = Collaborator();
        var manager = Manager();

        var task = new TaskItem
        {
            Id = 1,
            Description = "Tarefa",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Status = WorkTaskStatus.InProgress,
            AssignedToId = sub.Id,
            CreatedById = manager.Id,
            CreatedBy = manager,
            AssignedTo = sub
        };

        var taskMock = new Mock<ITaskRepository>();
        taskMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

        var controller = CreateController(sub, taskMock: taskMock);
        var model = new EditTaskStatusViewModel { Id = 1, Status = WorkTaskStatus.Pending };

        var result = await controller.Edit(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }
}
