using GestaoWeb.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace GestaoWeb.Models.ViewModels.Tasks;

public class EditTaskStatusViewModel
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Status")]
    public WorkTaskStatus Status { get; set; }
}
