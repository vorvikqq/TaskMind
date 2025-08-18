using TaskMind.Application.DTOs.TaskItem;
using TaskMind.Domain.Models;


namespace TaskMind.Application.Services.Interfaces
{
    public interface ITaskItemService
    {
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task<TaskItem?> GetByIdAsync(int id);
        Task<TaskItemCreateResponse> GetCreateModelAsync(CreateTaskItemRequest? dto = null);
        Task<TaskItemEditResponse?> GetEditModelAsync(int id, UpdateTaskItemRequest? dto = null);
        Task CreateAsync(CreateTaskItemRequest dto);
        Task UpdateAsync(UpdateTaskItemRequest dto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }

}
