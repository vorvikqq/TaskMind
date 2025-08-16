using TaskMind.Application.DTOs.TaskAssignments;

namespace TaskMind.Application.Services.Interfaces
{
    public interface ITaskAssignmentService
    {
        Task<TaskAssignmentResult> AssignEmployeeAsync(int taskId);
        Task<TaskAssignmentResult> UnassignEmployeeAsync(int taskId);
    }
}
