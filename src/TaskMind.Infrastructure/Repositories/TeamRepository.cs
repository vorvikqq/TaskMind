using Microsoft.EntityFrameworkCore;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Domain.Models;
using TaskMind.Infrastructure.Data;

namespace TaskMind.Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _context;

        public TeamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Team>> GetAllAsync()
        {
            return await _context.Teams
                .AsNoTracking()
                .Include(t => t.Employees)
                .Include(t => t.Tasks)
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<Team?> GetByIdAsync(int id)
        {
            return await _context.Teams
                .AsNoTracking()
                .Include(t => t.Employees)
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Team> CreateAsync(Team team)
        {
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<int> UpdateAsync(int id, Team team)
        {
            return await _context.Teams
                .Where(t => t.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Name, team.Name));
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _context.Teams
                 .Where(t => t.Id == id)
                 .ExecuteDeleteAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Teams.AnyAsync(t => t.Id == id);
        }
    }
}
