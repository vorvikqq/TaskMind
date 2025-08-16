using TaskMind.Domain.Models;

namespace TaskMind.Application.Services.Interfaces
{
    public interface ITeamService
    {
        Task<IEnumerable<Team>> GetAllAsync();
        Task<Team?> GetByIdAsync(int id);
        Task CreateAsync(Team team);
        Task UpdateAsync(Team team);
        Task DeleteAsync(int id);
        Task<IEnumerable<TaskItem>> GetTasksForTeamAsync(int teamId);
        bool TeamExists(int id);
    }
}
