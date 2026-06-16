using System.ComponentModel.DataAnnotations;
using GestaoWeb.Models.Validation;

namespace GestaoWeb.Models.ViewModels.Tasks;

public class CreateTaskViewModel
{
    [Required(ErrorMessage = "Informe a descrição")]
    [Display(Name = "Descrição")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data limite")]
    [Display(Name = "Data limite")]
    [MinToday]
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

    [Required(ErrorMessage = "Selecione o responsável")]
    [Display(Name = "Responsável")]
    public string AssignedToId { get; set; } = string.Empty;
}
