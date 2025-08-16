using TaskMind.Application.DTOs.TaskItem;
using TaskMind.Domain.Models;


namespace TaskMind.Application.Services.Interfaces
{
    public interface ITaskItemService
    {
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task<TaskItem?> GetByIdAsync(int id);
        Task CreateAsync(CreateTaskItemDto dto);
        Task UpdateAsync(int id, UpdateTaskItemDto dto);
        Task DeleteAsync(int id);
        bool TaskItemExists(int id);

        Task<IEnumerable<Team>> GetAllTeamsAsync();
        IEnumerable<SelectListItem> GetTaskStates();
    }

}
