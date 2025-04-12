using TaskMind.Application.DTOs.TaskAssignments;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Mappers
{
    public static class TaskAssignMapper
    {
        public static TaskRequestDto ToTaskRequestDto(TaskItem task, List<Employee> employees)
        {
            return new TaskRequestDto
            {
                Task = task.ToTaskDto(),
                Developers = employees.Select(e => e.ToDeveloperDto()).ToList()
            };
        }
        public static TaskDto ToTaskDto(this TaskItem task)
        {
            return new TaskDto
            {
                TaskID = task.Id,
                Difficulty = task.Difficulty,
                RequiredSkills = task.RequiredSkills,
                DeadlineDays = task.DeadlineDays,
                EstimatedHours = task.EstimatedHours
            };
        }

        public static DeveloperDto ToDeveloperDto(this Employee employee)
        {
            return new DeveloperDto
            {
                DeveloperID = employee.Id,
                DeveloperSkills = employee.Skills, 
                CurrentWorkload = employee.CurrentWorkload,
                TaskCompletionSpeed = employee.TaskCompletionSpeed
            };
        }
    }
}
