using TaskMind.Application.DTOs.TaskItem;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Mappers
{
    public static class TaskItemMapper
    {
        public static TaskItem ToTaskItemFromCreate(this CreateTaskItemDto createDto)
        {
            return new TaskItem
            {
                Title = createDto.Title,
                Description = createDto.Description,
                Difficulty = createDto.Difficulty,
                RequiredSkills = ParseSkills(createDto.RequiredSkills),
                DeadlineDays = createDto.DeadlineDays,
                EstimatedHours = createDto.EstimatedHours,
                TeamId = createDto.TeamId,
                Status = createDto.Status,
            };
        }
        public static void UpdateTaskItemFromDto(this TaskItem taskItem, UpdateTaskItemDto updateDto)
        {
            taskItem.Title = updateDto.Title;
            taskItem.Description = updateDto.Description;
            taskItem.Difficulty = updateDto.Difficulty;
            taskItem.RequiredSkills = ParseSkills(updateDto.RequiredSkills);
            taskItem.DeadlineDays = updateDto.DeadlineDays;
            taskItem.EstimatedHours = updateDto.EstimatedHours;
            taskItem.TeamId = updateDto.TeamId;
            taskItem.Status = updateDto.Status;
        }

        private static List<string> ParseSkills(string skills)
        {
            return skills.Split(',')
                         .Select(skill => skill.Trim())
                         .Where(skill => !string.IsNullOrEmpty(skill))
                         .ToList();
        }
    }
}
