using System.ComponentModel.DataAnnotations;

namespace GestaoWeb.Models.Validation;

public class MinTodayAttribute : ValidationAttribute
{
    public MinTodayAttribute() : base("A data limite não pode ser anterior a hoje.") { }

    public override bool IsValid(object? value)
        => value is DateOnly date && date >= DateOnly.FromDateTime(DateTime.Today);
}
