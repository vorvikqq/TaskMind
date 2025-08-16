using TaskMind.Domain.Models;

namespace TaskMind.Application.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<List<Employee>> GetByTeamIdAsync(int teamId);
        Task<Employee> CreateAsync(Employee employee);
        Task<Employee> UpdateAsync(Employee employee);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
