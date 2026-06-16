using GestaoWeb.Models.Domain;
using Microsoft.AspNetCore.Identity;

namespace GestaoWeb.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        const string email = "ti@leveinvestimentos.com.br";

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = "Administrador TI",
            MobilePhone = "11999990000",
            Address = "Sede",
            BirthDate = new DateOnly(1990, 1, 1),
            IsManager = true
        };

        await userManager.CreateAsync(user, "teste123");
    }
}
