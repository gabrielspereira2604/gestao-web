using System.ComponentModel.DataAnnotations;

namespace GestaoWeb.Models.ViewModels.Tasks;

public class CreateTaskViewModel
{
    [Required(ErrorMessage = "Informe a descrição")]
    [Display(Name = "Descrição")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data limite")]
    [Display(Name = "Data limite")]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Selecione o responsável")]
    [Display(Name = "Responsável")]
    public string AssignedToId { get; set; } = string.Empty;
}
