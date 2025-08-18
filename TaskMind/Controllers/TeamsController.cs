using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMind.Application.DTOs.Team;
using TaskMind.Application.Services.Interfaces;

namespace TaskMind.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
            => View(await _teamService.GetAllAsync());

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var team = await _teamService.GetByIdAsync(id.Value);
            if (team == null) return NotFound();

            return View(team);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamRequest dto)
        {
            if (!ModelState.IsValid) return View(dto);

            await _teamService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var editModel = await _teamService.GetForEditAsync(id.Value);
            if (editModel == null) return NotFound();

            return View(editModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateTeamRequest dto)
        {
            if (id != dto.Id) return NotFound();
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _teamService.UpdateAsync(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _teamService.ExistsAsync(dto.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
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
            try
            {
                await _teamService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageTasks(int? id)
        {
            if (id == null) return NotFound();

            var teamTasksModel = await _teamService.GetTeamTasksAsync(id.Value);
            if (teamTasksModel == null) return NotFound();

            return View(teamTasksModel);
        }
    }
}
