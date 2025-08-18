using Refit;
using TaskMind.Application.DTOs.TaskAssignments;

namespace TaskMind.Application.Services.Interfaces
{
    public interface ITaskAssignmentApi
    {
        [Post("/predict")]
        Task<BestDeveloperResponse> GetBestDeveloper([Body] TaskRequestDto request);
    }
}
