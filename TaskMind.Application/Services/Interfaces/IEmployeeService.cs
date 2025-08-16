using TaskMind.Application.DTOs.Employee;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task CreateEmployeeAsync(CreateEmployeeDto dto);
        Task UpdateEmployeeAsync(UpdateEmployeeDto dto);
        Task DeleteEmployeeAsync(int id);
        bool EmployeeExists(int id);
    }
}
