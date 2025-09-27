using TaskMind.Domain.Models;

namespace TaskMind.Application.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<List<Employee>> GetByTeamIdAsync(int teamId);
        Task<Employee> CreateAsync(Employee employee);
        Task<int> UpdateAsync(int id, Employee employee);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
