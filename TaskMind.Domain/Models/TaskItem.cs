using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskMind.Domain.Constants;

namespace TaskMind.Domain.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Difficulty { get; set; }
        public List<string> RequiredSkills { get; set; } = new List<string>();
        public int DeadlineDays { get; set; }
        public int EstimatedHours { get; set; }

        public TaskState Status { get; set; }
        public int TeamId { get; set; }
        public Team? Team { get; set; }
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

    }

}
