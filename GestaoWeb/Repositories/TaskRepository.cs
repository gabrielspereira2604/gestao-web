using GestaoWeb.Data;
using GestaoWeb.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace GestaoWeb.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _db;

    public TaskRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<TaskItem>> GetByManagerAsync(string managerId)
        => await _db.TaskItems
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Where(t => t.CreatedById == managerId)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

    public async Task<IEnumerable<TaskItem>> GetByAssigneeAsync(string userId)
        => await _db.TaskItems
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Where(t => t.AssignedToId == userId)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

    public async Task<TaskItem?> GetByIdAsync(int id)
        => await _db.TaskItems
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _db.TaskItems.Update(task);
        await _db.SaveChangesAsync();
    }
}
