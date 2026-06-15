using GestaoWeb.Models.Domain;
using GestaoWeb.Models.ViewModels.Users;
using GestaoWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GestaoWeb.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly IUserRepository _userRepo;
    private readonly ITaskRepository _taskRepo;
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedPhotoExtensions = [".jpg", ".jpeg", ".png", ".gif"];
    private const long MaxPhotoBytes = 2 * 1024 * 1024;

    public UsersController(IUserRepository userRepo, ITaskRepository taskRepo, UserManager<AppUser> userManager, IWebHostEnvironment env)
    {
        _userRepo = userRepo;
        _taskRepo = taskRepo;
        _userManager = userManager;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        if (!await IsManagerAsync()) return RedirectToAction("Index", "Home");
        var currentUser = await _userManager.GetUserAsync(User);
        ViewBag.CurrentUserId = currentUser?.Id;
        var users = await _userRepo.GetAllAsync();
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!await IsManagerAsync()) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!await IsManagerAsync()) return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid) return View(model);

        if (await _userManager.FindByEmailAsync(model.Email) is not null)
        {
            ModelState.AddModelError("Email", "Este e-mail já está em uso.");
            return View(model);
        }

        string? photoPath = await SavePhotoAsync(model.ProfilePhoto, ModelState);
        if (!ModelState.IsValid) return View(model);

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FullName = model.FullName,
            BirthDate = model.BirthDate,
            HomePhone = StripPhone(model.HomePhone),
            MobilePhone = StripPhone(model.MobilePhone),
            Address = model.Address,
            IsManager = model.IsManager,
            ProfilePhotoPath = photoPath
        };

        var result = await _userRepo.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        TempData["Success"] = "Usuário criado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "Account");
        if (!currentUser.IsManager && currentUser.Id != id) return RedirectToAction("Index", "Home");

        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound();

        var model = new EditUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            BirthDate = user.BirthDate,
            HomePhone = user.HomePhone,
            MobilePhone = user.MobilePhone,
            Address = user.Address,
            IsManager = user.IsManager,
            CurrentProfilePhotoPath = user.ProfilePhotoPath
        };

        ViewBag.IsManager = currentUser.IsManager;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "Account");
        if (!currentUser.IsManager && currentUser.Id != model.Id) return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            ViewBag.IsManager = currentUser.IsManager;
            return View(model);
        }

        var user = await _userRepo.GetByIdAsync(model.Id);
        if (user == null) return NotFound();

        var emailOwner = await _userManager.FindByEmailAsync(model.Email);
        if (emailOwner != null && emailOwner.Id != model.Id)
        {
            ModelState.AddModelError("Email", "Este e-mail já está em uso.");
            ViewBag.IsManager = currentUser.IsManager;
            return View(model);
        }

        string? oldPhotoPath = null;
        if (model.ProfilePhoto != null)
        {
            string? newPhotoPath = await SavePhotoAsync(model.ProfilePhoto, ModelState);
            if (!ModelState.IsValid)
            {
                ViewBag.IsManager = currentUser.IsManager;
                return View(model);
            }
            oldPhotoPath = user.ProfilePhotoPath;
            user.ProfilePhotoPath = newPhotoPath;
        }

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.NormalizedEmail = model.Email.ToUpperInvariant();
        user.NormalizedUserName = model.Email.ToUpperInvariant();
        user.BirthDate = model.BirthDate;
        user.HomePhone = StripPhone(model.HomePhone);
        user.MobilePhone = StripPhone(model.MobilePhone);
        user.Address = model.Address;

        if (currentUser.IsManager)
            user.IsManager = model.IsManager;

        var result = await _userRepo.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            ViewBag.IsManager = currentUser.IsManager;
            return View(model);
        }

        DeletePhotoFile(oldPhotoPath);

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var pwResult = await _userRepo.ChangePasswordAsync(user, model.NewPassword);
            if (!pwResult.Succeeded)
            {
                foreach (var error in pwResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                ViewBag.IsManager = currentUser.IsManager;
                return View(model);
            }
        }

        TempData["Success"] = "Usuário atualizado com sucesso.";
        return currentUser.IsManager
            ? RedirectToAction(nameof(Index))
            : RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (!await IsManagerAsync()) return RedirectToAction("Index", "Home");
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await IsManagerAsync()) return RedirectToAction("Index", "Home");

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Id == id)
        {
            TempData["Error"] = "Você não pode excluir sua própria conta.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound();

        var created  = await _taskRepo.GetByManagerAsync(id);
        var assigned = await _taskRepo.GetByAssigneeAsync(id);
        if (created.Any() || assigned.Any())
        {
            TempData["Error"] = $"Não é possível excluir {user.FullName} pois há tarefas vinculadas a este usuário.";
            return RedirectToAction(nameof(Index));
        }

        var photoPath = user.ProfilePhotoPath;

        var result = await _userRepo.DeleteAsync(user);
        if (!result.Succeeded)
        {
            TempData["Error"] = "Erro ao excluir usuário: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        DeletePhotoFile(photoPath);

        TempData["Success"] = $"Usuário {user.FullName} excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private void DeletePhotoFile(string? photoPath)
    {
        if (string.IsNullOrEmpty(photoPath)) return;
        var fullPath = Path.Combine(_env.WebRootPath, photoPath.TrimStart('/'));
        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
    }

    private static string? StripPhone(string? phone)
        => string.IsNullOrWhiteSpace(phone) ? null : new string(phone.Where(char.IsDigit).ToArray());

    private async Task<bool> IsManagerAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.IsManager == true;
    }

    private async Task<string?> SavePhotoAsync(IFormFile? file, ModelStateDictionary modelState)
    {
        if (file == null || file.Length == 0) return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedPhotoExtensions.Contains(ext))
        {
            modelState.AddModelError("ProfilePhoto", "Extensão não permitida. Use JPG, PNG ou GIF.");
            return null;
        }
        if (file.Length > MaxPhotoBytes)
        {
            modelState.AddModelError("ProfilePhoto", "A foto não pode ultrapassar 2 MB.");
            return null;
        }

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{fileName}";
    }
}
