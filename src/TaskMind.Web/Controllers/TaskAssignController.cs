using Microsoft.AspNetCore.Mvc;
using TaskMind.Application.Services.Interfaces;

namespace TaskMind.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskAssignController : ControllerBase
    {
        private readonly ITaskAssignmentService _taskAssignmentService;

        public TaskAssignController(ITaskAssignmentService taskAssignmentService)
        {
            _taskAssignmentService = taskAssignmentService;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignEmployee([FromQuery] int id)
        {
            try
            {
                var result = await _taskAssignmentService.AssignEmployeeAsync(id);

                if (!result.Success)
                    return BadRequest(new { message = result.ErrorMessage });

                return RedirectToAction("ManageTasks", "Teams", new { id = result.TeamId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("unassign")]
        public async Task<IActionResult> UnassignEmployee([FromQuery] int id)
        {
            try
            {
                var result = await _taskAssignmentService.UnassignEmployeeAsync(id);

                if (!result.Success)
                    return BadRequest(new { message = result.ErrorMessage });


                return RedirectToAction("ManageTasks", "Teams", new { id = result.TeamId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

}
