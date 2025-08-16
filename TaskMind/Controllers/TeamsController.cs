using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMind.Application.Services.Interfaces;
using TaskMind.Domain.Models;

namespace TaskMind.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        public async Task<IActionResult> Index()
            => View(await _teamService.GetAllAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var team = await _teamService.GetByIdAsync(id.Value);
            if (team == null) return NotFound();

            return View(team);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Team team)
        {
            if (!ModelState.IsValid) return View(team);

            await _teamService.CreateAsync(team);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var team = await _teamService.GetByIdAsync(id.Value);
            if (team == null) return NotFound();

            return View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Team team)
        {
            if (id != team.Id) return NotFound();
            if (!ModelState.IsValid) return View(team);

            try
            {
                await _teamService.UpdateAsync(team);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_teamService.TeamExists(team.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var team = await _teamService.GetByIdAsync(id.Value);
            if (team == null) return NotFound();

            return View(team);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _teamService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ManageTasks(int? id)
        {
            if (id == null) return NotFound();

            var team = await _teamService.GetByIdAsync(id.Value);
            if (team == null) return NotFound();

            var tasks = await _teamService.GetTasksForTeamAsync(team.Id);

            ViewData["Team"] = team;
            return View(tasks);
        }
    }
}
