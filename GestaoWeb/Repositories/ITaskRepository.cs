using GestaoWeb.Models.Domain;

namespace GestaoWeb.Repositories;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetByManagerAsync(string managerId);
    Task<IEnumerable<TaskItem>> GetByAssigneeAsync(string userId);
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
}
