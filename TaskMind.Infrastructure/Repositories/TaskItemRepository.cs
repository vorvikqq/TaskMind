using Microsoft.EntityFrameworkCore;
using TaskMind.Infrastructure.Data;
using TaskMind.Domain.Models;
using TaskMind.Application.Repositories.Interfaces;

namespace TaskMind.Infrastructure.Repositories
{
    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem?> DeleteAsync(int id)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id);

            if (task is null)
                return null;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return task;
        }

        public async Task<List<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks
                .Include(t => t.Team)
                .ToListAsync();
        }

        public async Task<TaskItem> GetByIdAsync(int? id)
        {
            return await _context.Tasks
                .Include(t => t.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<TaskItem>> GetByTeamIdAsync(int? teamId)
        {
            return await _context.Tasks
                .Include(t => t.Team)
                .Where(t=> t.TeamId == teamId)
                .ToListAsync();
        }

        public bool IsExist(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }

        public async Task<TaskItem> UpdateAsync(TaskItem task)
        {
            _context.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }
    }

}
