namespace TaskMind.Application.DTOs.TaskAssignments
{
    public class TaskRequestDto
    {
        public TaskDto Task { get; set; }
        public List<DeveloperDto> Developers { get; set; }
    }
}
