namespace TaskMind.Application.Services.Interfaces
{
    public interface ITaskAssignmentService
    {
        Task<bool> AssignEmployeeAsync(int taskId);
        Task<bool> UnassignEmployeeAsync(int taskId);
    }
}
