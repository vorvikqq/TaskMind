using TaskMind.Application.DTOs.TaskAssignments;

namespace TaskMind.Application.Services.Interfaces
{
    public interface ITaskAssignmentService
    {
        Task<TaskAssignmentResponse> AssignEmployeeAsync(int taskId);
        Task<TaskAssignmentResponse> UnassignEmployeeAsync(int taskId);
    }
}
