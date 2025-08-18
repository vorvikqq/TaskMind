namespace TaskMind.Application.DTOs.TaskAssignments
{
    public class DeveloperResponse
    {
        public int DeveloperID { get; set; }
        public List<string> DeveloperSkills { get; set; }
        public double CurrentWorkload { get; set; }
        public double TaskCompletionSpeed { get; set; }
    }
}
