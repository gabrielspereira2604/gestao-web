using Microsoft.AspNetCore.Identity;

namespace GestaoWeb.Models.Domain;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? HomePhone { get; set; }
    public string MobilePhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? ProfilePhotoPath { get; set; }
    public bool IsManager { get; set; }
}
