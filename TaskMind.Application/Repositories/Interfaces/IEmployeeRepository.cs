using TaskMind.Domain.Models;

namespace TaskMind.Application.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee> CreateAsync(Employee employee);
        Task<Employee> UpdateAsync(Employee employee);
        Task<Employee> DeleteAsync(int id);
        Task<List<Employee>> GetAllAsync();
        Task<Employee> GetByIdAsync(int? id);
        Task<List<Employee>> GetByTeamIdAsync(int? teamId);
        bool IsExist(int id);
    }
}
