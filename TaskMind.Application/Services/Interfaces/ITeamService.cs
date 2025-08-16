using TaskMind.Application.DTOs;
using TaskMind.Application.DTOs.Team;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Services.Interfaces
{
    public interface ITeamService
    {
        Task<IEnumerable<Team>> GetAllAsync();
        Task<Team?> GetByIdAsync(int id);
        Task<UpdateTeamDto?> GetForEditAsync(int id);
        Task<TeamTasksModel?> GetTeamTasksAsync(int teamId);
        Task CreateAsync(CreateTeamDto dto);
        Task UpdateAsync(UpdateTeamDto dto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
