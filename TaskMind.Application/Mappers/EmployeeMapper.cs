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

        public static void UpdateEmployeeFromDto(this Employee employee, UpdateEmployeeRequest updateEmployeeDto)
        {
            employee.Name = updateEmployeeDto.Name;
            employee.Skills = ParseSkills(updateEmployeeDto.Skills);
            employee.CurrentWorkload = updateEmployeeDto.CurrentWorkload;
            employee.TaskCompletionSpeed = updateEmployeeDto.TaskCompletionSpeed;
            employee.TeamId = updateEmployeeDto.TeamId;
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
