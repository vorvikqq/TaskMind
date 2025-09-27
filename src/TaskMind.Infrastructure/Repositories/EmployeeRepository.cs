using Microsoft.EntityFrameworkCore;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Domain.Models;
using TaskMind.Infrastructure.Data;

namespace TaskMind.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> CreateAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _context.Employees
                .Where(e => e.Id == id)
                .ExecuteDeleteAsync();

        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .AsNoTracking()
                .Include(e => e.Team)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .AsNoTracking()
                .Include(e => e.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Employee>> GetByTeamIdAsync(int teamId)
        {
            return await _context.Employees
                .AsNoTracking()
                .Include(e => e.Team)
                .Where(e => e.TeamId == teamId)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Employees.AnyAsync(e => e.Id == id);
        }

        public async Task<int> UpdateAsync(int id, Employee employee)
        {
            return await _context.Employees
                .Where(e => e.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Name, employee.Name)
                    .SetProperty(e => e.CurrentWorkload, employee.CurrentWorkload)
                    .SetProperty(e => e.TeamId, employee.TeamId)
                    .SetProperty(e => e.Skills, employee.Skills)
                    .SetProperty(e => e.TaskCompletionSpeed, employee.TaskCompletionSpeed)
                );
        }
    }

}
