using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskMind.Application.DTOs.Employee;
using TaskMind.Application.Mappers;
using TaskMind.Application.Repositories.Interfaces;

namespace TaskMind.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ITeamRepository _teamRepo;

        public EmployeesController(IEmployeeRepository employeeRepository, ITeamRepository teamRepo)
        {
            _employeeRepo = employeeRepository;
            _teamRepo = teamRepo;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            
            return View(await _employeeRepo.GetAllAsync());
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _employeeRepo.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeDto createEmployeeDto)
        {
            if (ModelState.IsValid)
            {
                var employee = createEmployeeDto.ToEmployeeFromCreate(); // Використовуємо маппер
                await _employeeRepo.CreateAsync(employee);
                return RedirectToAction(nameof(Index));
            }

            ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name", createEmployeeDto.TeamId);
            return View(createEmployeeDto);
        }


        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _employeeRepo.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var updateEmployeeDto = new UpdateEmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Skills = string.Join(", ", employee.Skills),
                CurrentWorkload = employee.CurrentWorkload,
                TaskCompletionSpeed = employee.TaskCompletionSpeed,
                TeamId = employee.TeamId
            };

            ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name", employee.TeamId);
            return View(updateEmployeeDto);
        }
        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateEmployeeDto updateEmployeeDto)
        {
            if (id != updateEmployeeDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var employee = await _employeeRepo.GetByIdAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }

                employee.UpdateEmployeeFromDto(updateEmployeeDto);

                try
                {
                   await _employeeRepo.UpdateAsync(employee);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name", updateEmployeeDto.TeamId);
            return View(updateEmployeeDto);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _employeeRepo.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _employeeRepo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _employeeRepo.IsExist(id);
        }
    }
}
