namespace TaskMind.Application.DTOs.TaskAssignments
{
    public class TaskRequestDto
    {
        public AssignTaskRequest Task { get; set; }
        public List<DeveloperResponse> Developers { get; set; }
    }
}
