using Refit;
using TaskMind.Application.DTOs.TaskAssignments;

namespace TaskMind.Infrastructure.Services.Interfaces
{
    public interface ITaskAssignmentApi
    {
        [Post("/predict")]
        Task<BestDeveloperResponse> GetBestDeveloper([Body] TaskRequestDto request);
    }
}
