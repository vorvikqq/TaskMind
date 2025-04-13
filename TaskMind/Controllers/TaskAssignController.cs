using Microsoft.AspNetCore.Mvc;
using TaskMind.Application.Mappers;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Domain.Models;
using TaskMind.Infrastructure.Services.Interfaces;

namespace TaskMind.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskAssignController : ControllerBase
    {
        private readonly ITaskAssignmentApi _api;
        private readonly ITaskItemRepository _taskItemRepo;
        private readonly IEmployeeRepository _employeeRepo;

        public TaskAssignController(ITaskAssignmentApi taskApi, ITaskItemRepository taskItemRepo, IEmployeeRepository employeeRepo)
        {
            _api = taskApi;
            _taskItemRepo = taskItemRepo;
            _employeeRepo = employeeRepo;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignEmployee([FromQuery] int id)
        {
            var task = await _taskItemRepo.GetByIdAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            var employees = await _employeeRepo.GetByTeamIdAsync(task.TeamId);

            var request = TaskAssignMapper.ToTaskRequestDto(task, employees);

            var response = await _api.GetBestDeveloper(request);

            if (response?.BestDeveloper == null)
            {
                return NotFound("No suitable developer found.");
            }

            var employee = await _employeeRepo.GetByIdAsync(response.BestDeveloper.DeveloperID);

            if (employee != null)
            {
                var deltaW = CalculateWorkloadChange(task, employee);
                employee.CurrentWorkload = Math.Min(Math.Round(employee.CurrentWorkload + deltaW, 2), 1);
                await _employeeRepo.UpdateAsync(employee);
            }

            task.EmployeeId = response.BestDeveloper.DeveloperID;
            await _taskItemRepo.UpdateAsync(task);

            return RedirectToAction("ManageTasks", "Teams", new { id = task.TeamId });
        }

        [HttpPost("unassign")]
        public async Task<IActionResult> UnassignEmployee([FromQuery] int id)
        {
            var task = await _taskItemRepo.GetByIdAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            if (task.EmployeeId.HasValue)
            {
                var employee = await _employeeRepo.GetByIdAsync(task.EmployeeId.Value);
                if (employee != null)
                {
                    var deltaW = CalculateWorkloadChange(task, employee);
                    employee.CurrentWorkload = Math.Max(0, Math.Round(employee.CurrentWorkload - deltaW, 2));
                    await _employeeRepo.UpdateAsync(employee);
                }
            }

            task.EmployeeId = null;
            await _taskItemRepo.UpdateAsync(task);

            return RedirectToAction("ManageTasks", "Teams", new { id = task.TeamId });
        }

        private double CalculateWorkloadChange(TaskItem task, Employee employee)
        {
            return (Math.Sqrt(task.EstimatedHours) * task.Difficulty) / ((task.DeadlineDays * employee.TaskCompletionSpeed) + 1);
        }

    }
}
