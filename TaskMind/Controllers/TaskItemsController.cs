using Microsoft.AspNetCore.Mvc;
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
            var createModel = await _taskItemService.GetCreateModelAsync();
            return View(createModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItemCreateModel model)
        {
            TryValidateModel(model.TaskItem, nameof(model.TaskItem));

            if (!ModelState.IsValid)
            {
                var createModel = await _taskItemService.GetCreateModelAsync(model.TaskItem);
                return View(createModel);
            }

            await _taskItemService.CreateAsync(model.TaskItem);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var editModel = await _taskItemService.GetEditModelAsync(id.Value);
            if (editModel == null) return NotFound();

            return View(editModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItemEditModel model)
        {
            if (id != model.TaskItem.Id) return NotFound();

            TryValidateModel(model.TaskItem, nameof(model.TaskItem));


            if (!ModelState.IsValid)
            {
                var editModel = await _taskItemService.GetEditModelAsync(id, model.TaskItem);
                return View(editModel);
            }

            try
            {
                await _taskItemService.UpdateAsync(model.TaskItem);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _taskItemService.ExistsAsync(model.TaskItem.Id)) return NotFound();
                throw;
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
            try
            {
                await _taskItemService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
