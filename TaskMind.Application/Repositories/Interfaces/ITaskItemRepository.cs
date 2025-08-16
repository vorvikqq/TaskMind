using TaskMind.Domain.Models;


namespace TaskMind.Application.Repositories.Interfaces
{
    public interface ITaskItemRepository
    {
        Task<List<TaskItem>> GetAllAsync();
        Task<TaskItem?> GetByIdAsync(int id);
        Task<List<TaskItem>> GetByTeamIdAsync(int teamId);
        Task<TaskItem> CreateAsync(TaskItem taskItem);
        Task<TaskItem> UpdateAsync(TaskItem taskItem);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
