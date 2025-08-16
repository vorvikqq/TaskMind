namespace TaskMind.Application.DTOs.TaskAssignments
{
    public class TaskAssignmentResult
    {
        public bool Success { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int? AssignedEmployeeId { get; private set; }
        public int TeamId { get; private set; }

        private TaskAssignmentResult() { }


        public static TaskAssignmentResult Successful(int? employeeId, int teamId)
        {
            return new TaskAssignmentResult
            {
                Success = true,
                AssignedEmployeeId = employeeId,
                TeamId = teamId
            };
        }

        public static TaskAssignmentResult Failed(string errorMessage)
        {
            return new TaskAssignmentResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
