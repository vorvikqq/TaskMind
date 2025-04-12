using TaskMind.Domain.Models;


namespace TaskMind.Application.Repositories.Interfaces
{
    public interface ITaskItemRepository
    {
        Task<TaskItem> CreateAsync(TaskItem taskItem);
        Task<TaskItem> UpdateAsync(TaskItem taskItem);
        Task<TaskItem> DeleteAsync(int id);
        Task<List<TaskItem>> GetAllAsync();
        Task<TaskItem> GetByIdAsync(int? id);
        Task<List<TaskItem>> GetByTeamIdAsync(int? teamId);
        bool IsExist(int id);
    }
}
