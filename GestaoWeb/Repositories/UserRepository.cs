using GestaoWeb.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestaoWeb.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<AppUser> _userManager;

    public UserRepository(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
        => await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();

    public async Task<AppUser?> GetByIdAsync(string id)
        => await _userManager.FindByIdAsync(id);

    public async Task<IdentityResult> CreateAsync(AppUser user, string password)
        => await _userManager.CreateAsync(user, password);

    public async Task<IdentityResult> UpdateAsync(AppUser user)
        => await _userManager.UpdateAsync(user);

    public async Task<IdentityResult> ChangePasswordAsync(AppUser user, string newPassword)
    {
        await _userManager.RemovePasswordAsync(user);
        return await _userManager.AddPasswordAsync(user, newPassword);
    }

    public async Task<IdentityResult> DeleteAsync(AppUser user)
        => await _userManager.DeleteAsync(user);
}
