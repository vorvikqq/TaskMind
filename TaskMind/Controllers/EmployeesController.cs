using Microsoft.AspNetCore.Mvc;
using TaskMind.Application.DTOs.Employee;
using TaskMind.Application.Services.Interfaces;

namespace TaskMind.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
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
            ViewData["Team"] = await _employeeService.GetTeamsForDropdownAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeRequest dto)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Team"] = await _employeeService.GetTeamsForDropdownAsync(dto.TeamId);
                return View(dto);
            }

            await _employeeService.CreateEmployeeAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var editModel = await _employeeService.GetEmployeeForEditAsync(id.Value);
            if (editModel == null) return NotFound();

            return View(editModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditEmployeeResponse model)
        {
            if (id != model.Employee.Id) return NotFound();

            TryValidateModel(model.Employee, nameof(model.Employee));

            if (!ModelState.IsValid)
            {
                model.Teams = await _employeeService.GetTeamsForDropdownAsync(model.Employee.TeamId);
                return View(model);
            }

            try
            {
                await _employeeService.UpdateEmployeeAsync(model.Employee);
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
            try
            {
                await _employeeService.DeleteEmployeeAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }

}
