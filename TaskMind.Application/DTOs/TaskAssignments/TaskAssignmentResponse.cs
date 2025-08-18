namespace TaskMind.Application.DTOs.TaskAssignments
{
    public class TaskAssignmentResponse
    {
        public bool Success { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int? AssignedEmployeeId { get; private set; }
        public int TeamId { get; private set; }

        private TaskAssignmentResponse() { }


        public static TaskAssignmentResponse Successful(int? employeeId, int teamId)
        {
            return new TaskAssignmentResponse
            {
                Success = true,
                AssignedEmployeeId = employeeId,
                TeamId = teamId
            };
        }

        public static TaskAssignmentResponse Failed(string errorMessage)
        {
            return new TaskAssignmentResponse
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
