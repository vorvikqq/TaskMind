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

        public async Task<Employee?> DeleteAsync(int id)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee is null)
                return null;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return employee;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees.Include(e => e.Team).ToListAsync();
        }

        public async Task<Employee> GetByIdAsync(int? id)
        {
            return await _context.Employees
                .Include(e => e.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Employee>> GetByTeamIdAsync(int? teamId)
        {
            return await _context.Employees
                .Include(e => e.Team)
                .Where(e => e.TeamId == teamId).ToListAsync();
        }

        public bool IsExist(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        public async Task<Employee> UpdateAsync(Employee employee)
        {
            _context.Update(employee);
            await _context.SaveChangesAsync();
            return employee;
        }
    }

}
