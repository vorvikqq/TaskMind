using Microsoft.AspNetCore.Mvc.Rendering;
using TaskMind.Application.DTOs.Employee;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<SelectList> GetTeamsForDropdownAsync(int? selectedTeamId = null);
        Task<EditEmployeeResponse?> GetEmployeeForEditAsync(int id);
        Task CreateEmployeeAsync(CreateEmployeeRequest dto);
        Task UpdateEmployeeAsync(UpdateEmployeeRequest dto);
        Task DeleteEmployeeAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
