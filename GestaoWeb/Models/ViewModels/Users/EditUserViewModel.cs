using System.ComponentModel.DataAnnotations;

namespace GestaoWeb.Models.ViewModels.Users;

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome completo")]
    [Display(Name = "Nome completo")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento")]
    [Display(Name = "Data de nascimento")]
    public DateOnly BirthDate { get; set; }

    [Display(Name = "Telefone fixo")]
    public string? HomePhone { get; set; }

    [Required(ErrorMessage = "Informe o celular")]
    [Display(Name = "Celular")]
    public string MobilePhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o endereço")]
    [Display(Name = "Endereço")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "Gestor")]
    public bool IsManager { get; set; }

    [Display(Name = "Nova foto de perfil")]
    public IFormFile? ProfilePhoto { get; set; }

    public string? CurrentProfilePhotoPath { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nova senha")]
    [Compare("NewPassword", ErrorMessage = "As senhas não conferem")]
    public string? ConfirmNewPassword { get; set; }
}
