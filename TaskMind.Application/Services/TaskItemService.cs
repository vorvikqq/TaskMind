using System.Web.Mvc;
using TaskMind.Application.DTOs.TaskItem;
using TaskMind.Application.Mappers;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Application.Services.Interfaces;
using TaskMind.Domain.Constants;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Services
{
    public class TaskItemService : ITaskItemService
    {
        private readonly ITaskItemRepository _taskItemRepo;
        private readonly ITeamRepository _teamRepo;
        private readonly List<SelectListItem> _taskStates;

        public TaskItemService(ITaskItemRepository taskItemRepo, ITeamRepository teamRepo)
        {
            _taskItemRepo = taskItemRepo;
            _teamRepo = teamRepo;

            _taskStates = Enum.GetValues(typeof(TaskState))
                              .Cast<TaskState>()
                              .Select(ts => new SelectListItem
                              {
                                  Value = ((int)ts).ToString(),
                                  Text = ts.ToString()
                              })
                              .ToList();
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync() => await _taskItemRepo.GetAllAsync();

        public async Task<TaskItem?> GetByIdAsync(int id) => await _taskItemRepo.GetByIdAsync(id);

        public async Task CreateAsync(CreateTaskItemDto dto)
        {
            var taskItem = dto.ToTaskItemFromCreate();
            await _taskItemRepo.CreateAsync(taskItem);
        }

        public async Task UpdateAsync(int id, UpdateTaskItemDto dto)
        {
            var taskItem = await _taskItemRepo.GetByIdAsync(id);
            if (taskItem == null)
                throw new KeyNotFoundException("Task not found");

            taskItem.UpdateTaskItemFromDto(dto);
            await _taskItemRepo.UpdateAsync(taskItem);
        }

        public Task DeleteAsync(int id) => _taskItemRepo.DeleteAsync(id);

        public bool TaskItemExists(int id) => _taskItemRepo.IsExist(id);

        public async Task<IEnumerable<Team>> GetAllTeamsAsync() => await _teamRepo.GetAllAsync();

        public IEnumerable<SelectListItem> GetTaskStates() => _taskStates;
    }

}
