namespace GestaoWeb.Models.Domain;

public enum WorkTaskStatus
{
    Pending,
    InProgress,
    Completed
}

public class TaskItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Pending;

    public string AssignedToId { get; set; } = string.Empty;
    public AppUser AssignedTo { get; set; } = null!;

    public string CreatedById { get; set; } = string.Empty;
    public AppUser CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
