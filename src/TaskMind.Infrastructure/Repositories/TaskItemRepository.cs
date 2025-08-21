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
                .AsNoTracking()
                .Include(t => t.Team)
                .Include(t => t.Employee)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Team)
                .Include(t => t.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<TaskItem>> GetByTeamIdAsync(int teamId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Team)
                .Include(t => t.Employee)
                .Where(t => t.TeamId == teamId)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<TaskItem> CreateAsync(TaskItem taskItem)
        {
            await _context.Tasks.AddAsync(taskItem);
            await _context.SaveChangesAsync();
            return taskItem;
        }

        public async Task<int> UpdateAsync(int id, TaskItem taskItem)
        {
            return await _context.Tasks
                .Where(t => t.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Title, taskItem.Title)
                    .SetProperty(t => t.Description, taskItem.Description)
                    .SetProperty(t => t.Difficulty, taskItem.Difficulty)
                    .SetProperty(t => t.RequiredSkills, taskItem.RequiredSkills)
                    .SetProperty(t => t.DeadlineDays, taskItem.DeadlineDays)
                    .SetProperty(t => t.EstimatedHours, taskItem.EstimatedHours)
                    .SetProperty(t => t.Status, taskItem.Status)
                    .SetProperty(t => t.TeamId, taskItem.TeamId)
                 );
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _context.Tasks
                .Where(t => t.Id == id)
                .ExecuteDeleteAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Tasks.AnyAsync(t => t.Id == id);
        }
    }

}
