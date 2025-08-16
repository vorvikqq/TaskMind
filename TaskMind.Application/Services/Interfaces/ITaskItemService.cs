using TaskMind.Application.DTOs.TaskItem;
using TaskMind.Domain.Models;


namespace TaskMind.Application.Services.Interfaces
{
    public interface ITaskItemService
    {
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task<TaskItem?> GetByIdAsync(int id);
        Task<TaskItemCreateModel> GetCreateModelAsync(CreateTaskItemDto? dto = null);
        Task<TaskItemEditModel?> GetEditModelAsync(int id, UpdateTaskItemDto? dto = null);
        Task CreateAsync(CreateTaskItemDto dto);
        Task UpdateAsync(UpdateTaskItemDto dto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }

}
