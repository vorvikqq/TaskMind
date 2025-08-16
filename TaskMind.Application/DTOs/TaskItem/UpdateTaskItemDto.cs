using System.ComponentModel.DataAnnotations;
using TaskMind.Domain.Constants;

namespace TaskMind.Application.DTOs.TaskItem
{
    public class UpdateTaskItemDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Range(0.0, 1.0, ErrorMessage = "Difficulty must be between 0.0 and 1.0")]
        public double Difficulty { get; set; }

        public string RequiredSkills { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Deadline must be greater than 0 days.")]
        public int DeadlineDays { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Estimated hours must be greater than 0.")]
        public int EstimatedHours { get; set; }
        public TaskState Status { get; set; }

        public int TeamId { get; set; }
        public Domain.Models.Team? Team { get; set; }
    }
}
