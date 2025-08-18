using TaskMind.Application.DTOs.TaskAssignments;
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
        private readonly IWorkloadCalculationService _workloadService;

        public TaskAssignmentService(
            ITaskAssignmentApi api,
            ITaskItemRepository taskItemRepo,
            IEmployeeRepository employeeRepo,
            IWorkloadCalculationService workloadService)
        {
            _api = api;
            _taskItemRepo = taskItemRepo;
            _employeeRepo = employeeRepo;
            _workloadService = workloadService;
        }

        public async Task<TaskAssignmentResponse> AssignEmployeeAsync(int taskId)
        {
            var task = await _taskItemRepo.GetByIdAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            if (task.EmployeeId.HasValue)
                return TaskAssignmentResponse.Failed("Task is already assigned");

            var employees = await _employeeRepo.GetByTeamIdAsync(task.TeamId);
            if (!employees.Any())
                return TaskAssignmentResponse.Failed("No employees available in the team");

            var request = TaskAssignMapper.ToTaskRequestDto(task, employees);
            var response = await _api.GetBestDeveloper(request);

            if (response?.BestDeveloper == null)
                return TaskAssignmentResponse.Failed("No suitable developer found");

            var employee = await _employeeRepo.GetByIdAsync(response.BestDeveloper.DeveloperID);
            if (employee == null)
                return TaskAssignmentResponse.Failed("Selected developer not found");

            // Update workload
            var deltaW = _workloadService.CalculateWorkloadChange(task, employee);
            employee.CurrentWorkload = Math.Min(Math.Round(employee.CurrentWorkload + deltaW, 2), 1);
            await _employeeRepo.UpdateAsync(employee);

            // Assign task
            task.EmployeeId = response.BestDeveloper.DeveloperID;
            await _taskItemRepo.UpdateAsync(task);

            return TaskAssignmentResponse.Successful(response.BestDeveloper.DeveloperID, task.TeamId);
        }

        public async Task<TaskAssignmentResponse> UnassignEmployeeAsync(int taskId)
        {
            var task = await _taskItemRepo.GetByIdAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            if (!task.EmployeeId.HasValue)
                return TaskAssignmentResponse.Failed("Task is not assigned to any employee");

            var employee = await _employeeRepo.GetByIdAsync(task.EmployeeId.Value);
            if (employee != null)
            {
                var deltaW = _workloadService.CalculateWorkloadChange(task, employee);
                employee.CurrentWorkload = Math.Max(0, Math.Round(employee.CurrentWorkload - deltaW, 2));
                await _employeeRepo.UpdateAsync(employee);
            }

            task.EmployeeId = null;
            await _taskItemRepo.UpdateAsync(task);

            return TaskAssignmentResponse.Successful(null, task.TeamId);
        }
    }

    public class WorkloadCalculationService : IWorkloadCalculationService
    {
        public double CalculateWorkloadChange(TaskItem task, Employee employee)
        {
            return (Math.Sqrt(task.EstimatedHours) * task.Difficulty) /
                   ((task.DeadlineDays * employee.TaskCompletionSpeed) + 1);
        }
    }
}
