using Microsoft.AspNetCore.Mvc.Rendering;
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

        public async Task<IEnumerable<TaskItem>> GetAllAsync()
            => await _taskItemRepo.GetAllAsync();

        public async Task<TaskItem?> GetByIdAsync(int id)
            => await _taskItemRepo.GetByIdAsync(id);

        public async Task<TaskItemCreateResponse> GetCreateModelAsync(CreateTaskItemRequest? dto = null)
        {
            var teams = await _teamRepo.GetAllAsync();

            return new TaskItemCreateResponse
            {
                TaskItem = dto ?? new CreateTaskItemRequest(),
                Teams = new SelectList(teams, "Id", "Name", dto?.TeamId),
                TaskStates = _taskStates
            };
        }

        public async Task<TaskItemEditResponse?> GetEditModelAsync(int id, UpdateTaskItemRequest? dto = null)
        {
            var taskItem = await _taskItemRepo.GetByIdAsync(id);
            if (taskItem == null) return null;

            var teams = await _teamRepo.GetAllAsync();

            var updateDto = dto ?? new UpdateTaskItemRequest
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                Difficulty = taskItem.Difficulty,
                RequiredSkills = string.Join(", ", taskItem.RequiredSkills),
                DeadlineDays = taskItem.DeadlineDays,
                EstimatedHours = taskItem.EstimatedHours,
                TeamId = taskItem.TeamId,
            };

            return new TaskItemEditResponse
            {
                TaskItem = updateDto,
                Teams = new SelectList(teams, "Id", "Name", updateDto.TeamId),
                TaskStates = _taskStates
            };
        }

        public async Task CreateAsync(CreateTaskItemRequest dto)
        {
            var taskItem = dto.ToTaskItemFromCreate();
            await _taskItemRepo.CreateAsync(taskItem);
        }

        public async Task UpdateAsync(UpdateTaskItemRequest dto)
        {
            var taskItem = await _taskItemRepo.GetByIdAsync(dto.Id);
            if (taskItem == null)
                throw new KeyNotFoundException("Task not found");

            taskItem.UpdateTaskItemFromDto(dto);
            await _taskItemRepo.UpdateAsync(taskItem);
        }

        public async Task DeleteAsync(int id)
        {
            var taskItem = await _taskItemRepo.GetByIdAsync(id);
            if (taskItem == null)
                throw new KeyNotFoundException("Task not found");

            await _taskItemRepo.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
            => await _taskItemRepo.ExistsAsync(id);
    }
}
