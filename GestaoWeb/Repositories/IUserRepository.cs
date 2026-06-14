using GestaoWeb.Models.Domain;
using Microsoft.AspNetCore.Identity;

namespace GestaoWeb.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<AppUser>> GetAllAsync();
    Task<AppUser?> GetByIdAsync(string id);
    Task<IdentityResult> CreateAsync(AppUser user, string password);
    Task<IdentityResult> UpdateAsync(AppUser user);
    Task<IdentityResult> ChangePasswordAsync(AppUser user, string newPassword);
}
