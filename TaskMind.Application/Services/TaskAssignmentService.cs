using TaskMind.Application.Mappers;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Application.Services.Interfaces;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Services
{
    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly ITaskAssignmentApi _api;
        private readonly ITaskItemRepository _taskItemRepo;
        private readonly IEmployeeRepository _employeeRepo;

        public TaskAssignmentService(ITaskAssignmentApi api, ITaskItemRepository taskItemRepo, IEmployeeRepository employeeRepo)
        {
            _api = api;
            _taskItemRepo = taskItemRepo;
            _employeeRepo = employeeRepo;
        }

        public async Task<bool> AssignEmployeeAsync(int taskId)
        {
            var task = await _taskItemRepo.GetByIdAsync(taskId);
            if (task == null) return false;

            var employees = await _employeeRepo.GetByTeamIdAsync(task.TeamId);
            var request = TaskAssignMapper.ToTaskRequestDto(task, employees);

            var response = await _api.GetBestDeveloper(request);
            if (response?.BestDeveloper == null) return false;

            var employee = await _employeeRepo.GetByIdAsync(response.BestDeveloper.DeveloperID);
            if (employee != null)
            {
                var deltaW = CalculateWorkloadChange(task, employee);
                employee.CurrentWorkload = Math.Min(Math.Round(employee.CurrentWorkload + deltaW, 2), 1);
                await _employeeRepo.UpdateAsync(employee);
            }

            task.EmployeeId = response.BestDeveloper.DeveloperID;
            await _taskItemRepo.UpdateAsync(task);

            return true;
        }

        public async Task<bool> UnassignEmployeeAsync(int taskId)
        {
            var task = await _taskItemRepo.GetByIdAsync(taskId);
            if (task == null) return false;

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

            return true;
        }

        private double CalculateWorkloadChange(TaskItem task, Employee employee)
        {
            return (Math.Sqrt(task.EstimatedHours) * task.Difficulty) /
                   ((task.DeadlineDays * employee.TaskCompletionSpeed) + 1);
        }
    }

}
