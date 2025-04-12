using System.Text.Json.Serialization;

namespace TaskMind.Domain.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new List<string>();
        public double CurrentWorkload { get; set; } = 0;
        public double TaskCompletionSpeed { get; set; }
        public int? TeamId { get; set; }
        public Team? Team { get; set; }
    }
}
