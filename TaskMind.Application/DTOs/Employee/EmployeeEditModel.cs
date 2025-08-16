using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace TaskMind.Application.DTOs.Employee
{
    public class EmployeeEditModel
    {
        public UpdateEmployeeDto Employee { get; set; } = null!;
        [ValidateNever]
        public SelectList Teams { get; set; } = null!;
    }
}
