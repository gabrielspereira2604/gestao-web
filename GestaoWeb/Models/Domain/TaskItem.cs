namespace GestaoWeb.Models.Domain;

public enum TaskStatus
{
    Pending,
    InProgress,
    Completed
}

public class TaskItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    public string AssignedToId { get; set; } = string.Empty;
    public AppUser AssignedTo { get; set; } = null!;

    public string CreatedById { get; set; } = string.Empty;
    public AppUser CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
