using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace TaskMind.Application.DTOs.Employee
{
    public class EditEmployeeResponse
    {
        public UpdateEmployeeRequest Employee { get; set; } = null!;
        [ValidateNever]
        public SelectList Teams { get; set; } = null!;
    }
}
