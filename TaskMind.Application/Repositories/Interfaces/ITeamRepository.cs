using TaskMind.Domain.Models;

namespace TaskMind.Application.Repositories.Interfaces
{
    public interface ITeamRepository
    {
        Task<Team> CreateAsync(Team team);
        Task<Team> UpdateAsync(Team team);
        Task<Team> DeleteAsync(int id);
        Task<List<Team>> GetAllAsync();
        Task<Team> GetByIdAsync(int? id);
        bool IsExist(int id);
    }
}
