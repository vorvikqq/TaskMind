using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskMind.Application.DTOs.Employee;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Application.Services.Interfaces;

namespace TaskMind.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly ITeamRepository _teamRepo;

        public EmployeesController(IEmployeeService employeeService, ITeamRepository teamRepo)
        {
            _employeeService = employeeService;
            _teamRepo = teamRepo;
        }

        public async Task<IActionResult> Index()
            => View(await _employeeService.GetAllEmployeesAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            return View(employee);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name", dto.TeamId);
                return View(dto);
            }

            await _employeeService.CreateEmployeeAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            var dto = new UpdateEmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Skills = string.Join(", ", employee.Skills),
                CurrentWorkload = employee.CurrentWorkload,
                TaskCompletionSpeed = employee.TaskCompletionSpeed,
                TeamId = employee.TeamId
            };

            ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name", employee.TeamId);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateEmployeeDto dto)
        {
            if (id != dto.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name", dto.TeamId);
                return View(dto);
            }

            try
            {
                await _employeeService.UpdateEmployeeAsync(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }

}
