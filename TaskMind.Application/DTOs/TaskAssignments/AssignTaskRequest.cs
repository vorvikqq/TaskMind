namespace TaskMind.Application.DTOs.TaskAssignments
{
    public class AssignTaskRequest
    {
        public int TaskID { get; set; }
        public double Difficulty { get; set; }
        public List<string> RequiredSkills { get; set; }
        public int DeadlineDays { get; set; }
        public int EstimatedHours { get; set; }
    }
}
