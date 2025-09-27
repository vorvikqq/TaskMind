using TaskMind.Application.DTOs.TaskItem;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Mappers
{
    public static class TaskItemMapper
    {
        public static TaskItem ToTaskItemFromCreate(this CreateTaskItemRequest createDto)
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

        public static TaskItem ToTaskItemFromUpdate(this UpdateTaskItemRequest updateDto)
        {
            return new TaskItem
            {
                Id = updateDto.Id,
                Title = updateDto.Title,
                Description = updateDto.Description,
                Difficulty = updateDto.Difficulty,
                RequiredSkills = ParseSkills(updateDto.RequiredSkills),
                DeadlineDays = updateDto.DeadlineDays,
                EstimatedHours = updateDto.EstimatedHours,
                TeamId = updateDto.TeamId,
                Status = updateDto.Status,
            };
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
