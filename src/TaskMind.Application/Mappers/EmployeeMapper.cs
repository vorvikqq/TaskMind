using TaskMind.Application.DTOs.Employee;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Mappers
{
    public static class EmployeeMapper
    {
        public static Employee ToEmployeeFromCreate(this CreateEmployeeRequest createEmployeeDto)
        {
            return new Employee
            {
                Name = createEmployeeDto.Name,
                Skills = ParseSkills(createEmployeeDto.Skills),
                CurrentWorkload = createEmployeeDto.CurrentWorkload,
                TaskCompletionSpeed = createEmployeeDto.TaskCompletionSpeed,
                TeamId = createEmployeeDto.TeamId
            };
        }

        public static Employee ToEmployeeFromUpdate(this UpdateEmployeeRequest createEmployeeDto)
        {
            return new Employee
            {
                Name = createEmployeeDto.Name,
                Skills = ParseSkills(createEmployeeDto.Skills),
                CurrentWorkload = createEmployeeDto.CurrentWorkload,
                TaskCompletionSpeed = createEmployeeDto.TaskCompletionSpeed,
                TeamId = createEmployeeDto.TeamId
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
