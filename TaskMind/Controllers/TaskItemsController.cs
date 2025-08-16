using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskMind.Application.DTOs.TaskItem;
using TaskMind.Application.Services.Interfaces;

namespace TaskMind.Controllers
{
    public class TaskItemsController : Controller
    {
        private readonly ITaskItemService _taskItemService;

        public TaskItemsController(ITaskItemService taskItemService)
        {
            _taskItemService = taskItemService;
        }

        public async Task<IActionResult> Index()
            => View(await _taskItemService.GetAllAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _taskItemService.GetByIdAsync(id.Value);
            if (taskItem == null) return NotFound();

            return View(taskItem);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["TaskState"] = _taskItemService.GetTaskStates();
            ViewData["Team"] = new SelectList(await _taskItemService.GetAllTeamsAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewData["TaskState"] = _taskItemService.GetTaskStates();
                ViewData["Team"] = new SelectList(await _taskItemService.GetAllTeamsAsync(), "Id", "Name", dto.TeamId);
                return View(dto);
            }

            await _taskItemService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _taskItemService.GetByIdAsync(id.Value);
            if (taskItem == null) return NotFound();

            var updateDto = new UpdateTaskItemDto
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                Difficulty = taskItem.Difficulty,
                RequiredSkills = string.Join(", ", taskItem.RequiredSkills),
                DeadlineDays = taskItem.DeadlineDays,
                EstimatedHours = taskItem.EstimatedHours,
                TeamId = taskItem.TeamId,
            };

            ViewData["TaskState"] = _taskItemService.GetTaskStates();
            ViewData["Team"] = new SelectList(await _taskItemService.GetAllTeamsAsync(), "Id", "Name", taskItem.TeamId);
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateTaskItemDto dto)
        {
            if (id != dto.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["TaskState"] = _taskItemService.GetTaskStates();
                ViewData["Team"] = new SelectList(await _taskItemService.GetAllTeamsAsync(), "Id", "Name", dto.TeamId);
                return View(dto);
            }

            try
            {
                await _taskItemService.UpdateAsync(id, dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_taskItemService.TaskItemExists(dto.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _taskItemService.GetByIdAsync(id.Value);
            if (taskItem == null) return NotFound();

            return View(taskItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _taskItemService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
