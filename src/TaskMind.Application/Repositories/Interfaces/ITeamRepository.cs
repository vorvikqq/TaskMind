using TaskMind.Domain.Models;

namespace TaskMind.Application.Repositories.Interfaces
{
    public interface ITeamRepository
    {
        Task<List<Team>> GetAllAsync();
        Task<Team?> GetByIdAsync(int id);
        Task<Team> CreateAsync(Team team);
        Task<int> UpdateAsync(int id, Team team);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
