using System.ComponentModel.DataAnnotations;

namespace TaskMind.Application.DTOs.Employee
{
    public class CreateEmployeeDto
    {
        [Required]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters long.")]
        [MaxLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Range(0.0, 1.0, ErrorMessage = "Current workload must be between 0.0 and 1.0")]
        public double CurrentWorkload { get; set; } = 0;

        [Range(0.01, 1.0, ErrorMessage = "Task completion speed must be greater than 0.01")]
        public double TaskCompletionSpeed { get; set; }
        public string Skills { get; set; } = string.Empty;
        public int? TeamId { get; set; }
        public Domain.Models.Team? Team { get; set; }

    }
}
