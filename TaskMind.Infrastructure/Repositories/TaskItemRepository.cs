using Microsoft.EntityFrameworkCore;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Domain.Models;
using TaskMind.Infrastructure.Data;

namespace TaskMind.Infrastructure.Repositories
{
    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks
                .Include(t => t.Team)
                .Include(t => t.Employee)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Team)
                .Include(t => t.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<TaskItem>> GetByTeamIdAsync(int teamId)
        {
            return await _context.Tasks
                .Include(t => t.Team)
                .Include(t => t.Employee)
                .Where(t => t.TeamId == teamId)
                .ToListAsync();
        }

        public async Task<TaskItem> CreateAsync(TaskItem taskItem)
        {
            await _context.Tasks.AddAsync(taskItem);
            await _context.SaveChangesAsync();
            return taskItem;
        }

        public async Task<TaskItem> UpdateAsync(TaskItem taskItem)
        {
            _context.Update(taskItem);
            await _context.SaveChangesAsync();
            return taskItem;
        }

        public async Task DeleteAsync(int id)
        {
            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem != null)
            {
                _context.Tasks.Remove(taskItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Tasks.AnyAsync(t => t.Id == id);
        }
    }

}
