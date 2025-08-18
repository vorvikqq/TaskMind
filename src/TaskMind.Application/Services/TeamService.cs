using TaskMind.Application.DTOs;
using TaskMind.Application.DTOs.Team;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Application.Services.Interfaces;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepo;
        private readonly ITaskItemRepository _taskItemRepo;

        public TeamService(ITeamRepository teamRepo, ITaskItemRepository taskItemRepo)
        {
            _teamRepo = teamRepo;
            _taskItemRepo = taskItemRepo;
        }

        public async Task<IEnumerable<Team>> GetAllAsync()
            => await _teamRepo.GetAllAsync();

        public async Task<Team?> GetByIdAsync(int id)
            => await _teamRepo.GetByIdAsync(id);

        public async Task<UpdateTeamRequest?> GetForEditAsync(int id)
        {
            var team = await _teamRepo.GetByIdAsync(id);
            return team == null ? null : new UpdateTeamRequest { Id = team.Id, Name = team.Name };
        }

        public async Task<TeamTaskResponse?> GetTeamTasksAsync(int teamId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId);
            if (team == null) return null;

            var tasks = await _taskItemRepo.GetByTeamIdAsync(teamId);

            return new TeamTaskResponse
            {
                Team = team,
                Tasks = tasks
            };
        }

        public async Task CreateAsync(CreateTeamRequest dto)
        {
            var team = new Team { Name = dto.Name };
            await _teamRepo.CreateAsync(team);
        }

        public async Task UpdateAsync(UpdateTeamRequest dto)
        {
            var team = await _teamRepo.GetByIdAsync(dto.Id);
            if (team == null)
                throw new KeyNotFoundException("Team not found");

            team.Name = dto.Name;
            await _teamRepo.UpdateAsync(team);
        }

        public async Task DeleteAsync(int id)
        {
            var team = await _teamRepo.GetByIdAsync(id);
            if (team == null)
                throw new KeyNotFoundException("Team not found");

            await _teamRepo.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
            => await _teamRepo.ExistsAsync(id);
    }
}
