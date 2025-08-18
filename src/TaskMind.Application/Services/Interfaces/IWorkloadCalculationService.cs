using TaskMind.Domain.Models;

namespace TaskMind.Application.Services.Interfaces
{
    public interface IWorkloadCalculationService
    {
        double CalculateWorkloadChange(TaskItem task, Employee employee);
    }
}
