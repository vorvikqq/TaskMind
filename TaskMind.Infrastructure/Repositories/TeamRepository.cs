using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using TaskMind.Infrastructure.Data;
using TaskMind.Domain.Models;
using TaskMind.Application.Repositories.Interfaces;

namespace TaskMind.Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _context;
        public TeamRepository(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task<Team> CreateAsync(Team team)
        {
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<Team?> DeleteAsync(int id)
        {
            var team = await _context.Teams
               .FirstOrDefaultAsync(m => m.Id == id);

            if (team is null)
                return null;

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return team;
        }

        public async Task<List<Team>> GetAllAsync()
        {
            return await _context.Teams
                .Include(t => t.Employees)
                .Include(t => t.Tasks)
                .ToListAsync();
        }

        public async Task<Team> GetByIdAsync(int? id)
        {
            return await _context.Teams
                .Include(t=> t.Employees)
                .Include(t=> t.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public bool IsExist(int id)
        {
            return _context.Teams.Any(e => e.Id == id);

        }

        public async Task<Team> UpdateAsync(Team team)
        {
            _context.Update(team);
            await _context.SaveChangesAsync();
            return team;
        }

    }
}
