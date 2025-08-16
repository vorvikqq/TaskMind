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

        public EmployeeService(IEmployeeRepository employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
            => await _employeeRepo.GetAllAsync();

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
            => await _employeeRepo.GetByIdAsync(id);

        public async Task CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            var employee = dto.ToEmployeeFromCreate();
            await _employeeRepo.CreateAsync(employee);
        }

        public async Task UpdateEmployeeAsync(UpdateEmployeeDto dto)
        {
            var employee = await _employeeRepo.GetByIdAsync(dto.Id);
            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            employee.UpdateEmployeeFromDto(dto);
            await _employeeRepo.UpdateAsync(employee);
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            await _employeeRepo.DeleteAsync(id);
        }

        public bool EmployeeExists(int id) => _employeeRepo.IsExist(id);
    }

}
