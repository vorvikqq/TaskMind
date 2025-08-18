using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMind.Domain.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Employee> Employees { get; set; } = new List<Employee>();
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();


    }
}
