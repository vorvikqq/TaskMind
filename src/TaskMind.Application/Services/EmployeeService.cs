using Microsoft.AspNetCore.Mvc.Rendering;
using TaskMind.Application.DTOs.Employee;
using TaskMind.Application.Mappers;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Application.Services.Interfaces;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ITeamRepository _teamRepo;

        public EmployeeService(IEmployeeRepository employeeRepo, ITeamRepository teamRepo)
        {
            _employeeRepo = employeeRepo;
            _teamRepo = teamRepo;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
            => await _employeeRepo.GetAllAsync();

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
            => await _employeeRepo.GetByIdAsync(id);

        public async Task<SelectList> GetTeamsForDropdownAsync(int? selectedTeamId = null)
        {
            var teams = await _teamRepo.GetAllAsync();
            return new SelectList(teams, "Id", "Name", selectedTeamId);
        }

        public async Task<EditEmployeeResponse?> GetEmployeeForEditAsync(int id)
        {
            var employee = await _employeeRepo.GetByIdAsync(id);
            if (employee == null) return null;

            var teams = await GetTeamsForDropdownAsync(employee.TeamId);

            return new EditEmployeeResponse
            {
                Employee = new UpdateEmployeeRequest
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Skills = string.Join(", ", employee.Skills),
                    CurrentWorkload = employee.CurrentWorkload,
                    TaskCompletionSpeed = employee.TaskCompletionSpeed,
                    TeamId = employee.TeamId
                },
                Teams = teams
            };
        }

        public async Task CreateEmployeeAsync(CreateEmployeeRequest dto)
        {
            var employee = dto.ToEmployeeFromCreate();
            await _employeeRepo.CreateAsync(employee);
        }

        public async Task UpdateEmployeeAsync(UpdateEmployeeRequest dto)
        {
            var employee = dto.ToEmployeeFromUpdate();

            var updatedCount = await _employeeRepo.UpdateAsync(dto.Id, employee);
            if (updatedCount == 0)
                throw new KeyNotFoundException("Employee not found");

        }

        public async Task DeleteEmployeeAsync(int id)
        {
            var deletedCount = await _employeeRepo.DeleteAsync(id);

            if (deletedCount == 0)
                throw new KeyNotFoundException("Employee not found");
        }

        public async Task<bool> ExistsAsync(int id)
            => await _employeeRepo.ExistsAsync(id);
    }

}
