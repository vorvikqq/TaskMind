namespace TaskMind.Application.DTOs
{
    public class TeamTasksModel
    {
        public Domain.Models.Team Team { get; set; } = null!;
        public IEnumerable<Domain.Models.TaskItem> Tasks { get; set; } = null!;
    }
}
