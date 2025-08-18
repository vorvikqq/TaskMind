using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskMind.Application.DTOs.TaskItem
{
    public class TaskItemEditResponse
    {
        public UpdateTaskItemRequest TaskItem { get; set; } = null!;
        [ValidateNever]
        public SelectList Teams { get; set; } = null!;
        [ValidateNever]
        public IEnumerable<SelectListItem> TaskStates { get; set; } = null!;
    }
}
