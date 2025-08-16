using Microsoft.AspNetCore.Mvc;
using TaskMind.Application.Services.Interfaces;

namespace TaskMind.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskAssignController : ControllerBase
    {
        private readonly ITaskAssignmentService _taskAssignmentService;
        private readonly ITaskItemService _taskItemService;

        public TaskAssignController(ITaskAssignmentService taskAssignmentService, ITaskItemService taskItemService)
        {
            _taskAssignmentService = taskAssignmentService;
            _taskItemService = taskItemService;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignEmployee([FromQuery] int id)
        {
            var success = await _taskAssignmentService.AssignEmployeeAsync(id);
            var task = await _taskItemService.GetByIdAsync(id);
            if (!success) return NotFound("No suitable developer found or task not found.");

            return RedirectToAction("ManageTasks", "Teams", new { id = task!.TeamId });
        }

        [HttpPost("unassign")]
        public async Task<IActionResult> UnassignEmployee([FromQuery] int id)
        {
            var success = await _taskAssignmentService.UnassignEmployeeAsync(id);
            var task = await _taskItemService.GetByIdAsync(id);

            if (!success) return NotFound("Task not found.");

            return RedirectToAction("ManageTasks", "Teams", new { id = task!.TeamId });
        }
    }

}
