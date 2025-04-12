using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskMind.Application.DTOs.TaskItem;
using TaskMind.Application.Mappers;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Domain.Constants;

namespace TaskMind.Controllers
{
    public class TaskItemsController : Controller
    {
        private readonly ITaskItemRepository _taskItemRepo;
        private readonly ITeamRepository _teamRepo;
        private readonly List<SelectListItem> _taskStates;

        public TaskItemsController(ITaskItemRepository taskItemRepository, ITeamRepository teamRepo)
        {
            _taskItemRepo = taskItemRepository;
            _teamRepo = teamRepo;

            _taskStates = Enum.GetValues(typeof(TaskState))
                              .Cast<TaskState>()
                              .Select(ts => new SelectListItem
                              {
                                  Value = ((int)ts).ToString(),
                                  Text = ts.ToString()
                              })
                              .ToList();
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            return View(await _taskItemRepo.GetAllAsync());
        }

        // GET: TaskItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _taskItemRepo.GetByIdAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // GET: TaskItems/Create
        public async Task<IActionResult> Create()
        {
            ViewData["TaskState"] = _taskStates;
            ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name");
            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskItemDto createTaskItemDto)
        {
            if (ModelState.IsValid)
            {
                var taskItem = createTaskItemDto.ToTaskItemFromCreate();
                await _taskItemRepo.CreateAsync(taskItem);
                return RedirectToAction(nameof(Index));
            }

            ViewData["TaskState"] = _taskStates;
            ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name", createTaskItemDto.TeamId);
            return View(createTaskItemDto);
        }

        // GET: TaskItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _taskItemRepo.GetByIdAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

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

            ViewData["TaskState"] = _taskStates;
            ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name", taskItem.TeamId);
            return View(updateDto);
        }

        // POST: TaskItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateTaskItemDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var taskItem = await _taskItemRepo.GetByIdAsync(id);
                if (taskItem == null)
                {
                    return NotFound();
                }

                taskItem.UpdateTaskItemFromDto(updateDto);

                try
                {
                    await _taskItemRepo.UpdateAsync(taskItem);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["TaskState"] = _taskStates;
            ViewData["Team"] = new SelectList(await _teamRepo.GetAllAsync(), "Id", "Name", updateDto.TeamId);
            return View(updateDto);
        }

        // GET: TaskItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _taskItemRepo.GetByIdAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _taskItemRepo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool TaskItemExists(int id)
        {
            return _taskItemRepo.IsExist(id);
        }
    }
}
