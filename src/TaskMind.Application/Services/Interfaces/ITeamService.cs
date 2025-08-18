using TaskMind.Application.DTOs;
using TaskMind.Application.DTOs.Team;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Services.Interfaces
{
    public interface ITeamService
    {
        Task<IEnumerable<Team>> GetAllAsync();
        Task<Team?> GetByIdAsync(int id);
        Task<UpdateTeamRequest?> GetForEditAsync(int id);
        Task<TeamTaskResponse?> GetTeamTasksAsync(int teamId);
        Task CreateAsync(CreateTeamRequest dto);
        Task UpdateAsync(UpdateTeamRequest dto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
