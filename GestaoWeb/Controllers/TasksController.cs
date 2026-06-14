using GestaoWeb.Models.Domain;
using GestaoWeb.Models.ViewModels.Tasks;
using GestaoWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestaoWeb.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ITaskRepository _taskRepo;
    private readonly IUserRepository _userRepo;
    private readonly UserManager<AppUser> _userManager;

    public TasksController(ITaskRepository taskRepo, IUserRepository userRepo, UserManager<AppUser> userManager)
    {
        _taskRepo = taskRepo;
        _userRepo = userRepo;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "Account");

        if (currentUser.IsManager)
        {
            var tasks = await _taskRepo.GetByManagerAsync(currentUser.Id);
            ViewBag.IsManager = true;
            return View(tasks);
        }
        else
        {
            var tasks = await _taskRepo.GetByAssigneeAsync(currentUser.Id);
            ViewBag.IsManager = false;
            return View(tasks);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!await IsManagerAsync()) return RedirectToAction("Index", "Home");

        await PopulateSubordinatesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTaskViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || !currentUser.IsManager) return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            await PopulateSubordinatesAsync();
            return View(model);
        }

        var task = new TaskItem
        {
            Description = model.Description,
            DueDate = model.DueDate,
            AssignedToId = model.AssignedToId,
            CreatedById = currentUser.Id,
            Status = WorkTaskStatus.Pending
        };

        await _taskRepo.CreateAsync(task);

        TempData["Success"] = "Tarefa criada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "Account");

        var task = await _taskRepo.GetByIdAsync(id);
        if (task == null) return NotFound();

        if (currentUser.IsManager && task.CreatedById != currentUser.Id) return RedirectToAction(nameof(Index));
        if (!currentUser.IsManager && task.AssignedToId != currentUser.Id) return RedirectToAction(nameof(Index));

        var model = new EditTaskStatusViewModel
        {
            Id = task.Id,
            Description = task.Description,
            DueDate = task.DueDate,
            Status = task.Status,
            AssignedToName = task.AssignedTo.FullName,
            CreatedByName = task.CreatedBy.FullName
        };

        ViewBag.IsManager = currentUser.IsManager;
        ViewBag.AvailableStatuses = GetAvailableStatuses(task.Status, currentUser.IsManager);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTaskStatusViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "Account");

        var task = await _taskRepo.GetByIdAsync(model.Id);
        if (task == null) return NotFound();

        if (currentUser.IsManager && task.CreatedById != currentUser.Id) return RedirectToAction(nameof(Index));
        if (!currentUser.IsManager && task.AssignedToId != currentUser.Id) return RedirectToAction(nameof(Index));

        if (!currentUser.IsManager && model.Status < task.Status)
        {
            ModelState.AddModelError("Status", "O status não pode regredir.");
            ViewBag.IsManager = false;
            ViewBag.AvailableStatuses = GetAvailableStatuses(task.Status, false);
            return View(model);
        }

        task.Status = model.Status;
        if (model.Status == WorkTaskStatus.Completed && task.CompletedAt == null)
            task.CompletedAt = DateTime.UtcNow;

        await _taskRepo.UpdateAsync(task);

        TempData["Success"] = "Status atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "Account");

        var task = await _taskRepo.GetByIdAsync(id);
        if (task == null) return NotFound();

        if (currentUser.IsManager && task.CreatedById != currentUser.Id) return RedirectToAction(nameof(Index));
        if (!currentUser.IsManager && task.AssignedToId != currentUser.Id) return RedirectToAction(nameof(Index));

        ViewBag.IsManager = currentUser.IsManager;
        return View(task);
    }

    private async Task<bool> IsManagerAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.IsManager == true;
    }

    private async Task PopulateSubordinatesAsync()
    {
        var allUsers = await _userRepo.GetAllAsync();
        var subordinates = allUsers.Where(u => !u.IsManager).ToList();
        ViewBag.Subordinates = new SelectList(subordinates, "Id", "FullName");
    }

    private static SelectList GetAvailableStatuses(WorkTaskStatus current, bool isManager)
    {
        var all = new[]
        {
            new { Value = WorkTaskStatus.Pending,    Text = "Pendente" },
            new { Value = WorkTaskStatus.InProgress, Text = "Em andamento" },
            new { Value = WorkTaskStatus.Completed,  Text = "Concluída" }
        };

        var available = isManager
            ? all
            : all.Where(s => s.Value >= current).ToArray();

        return new SelectList(available, "Value", "Text", current);
    }
}
