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

        public async Task<IEnumerable<Team>> GetAllAsync() => await _teamRepo.GetAllAsync();
        public async Task<Team?> GetByIdAsync(int id) => await _teamRepo.GetByIdAsync(id);
        public async Task CreateAsync(Team team) => await _teamRepo.CreateAsync(team);
        public async Task UpdateAsync(Team team) => await _teamRepo.UpdateAsync(team);
        public async Task DeleteAsync(int id) => await _teamRepo.DeleteAsync(id);
        public async Task<IEnumerable<TaskItem>> GetTasksForTeamAsync(int teamId) => await _taskItemRepo.GetByTeamIdAsync(teamId);
        public bool TeamExists(int id) => _teamRepo.IsExist(id);
    }

}
