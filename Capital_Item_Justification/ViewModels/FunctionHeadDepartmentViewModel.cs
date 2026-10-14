using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class FunctionHeadDepartmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a department.")]
        public int? DepartmentId { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? DepartmentName { get; set; }

        //public string? RoleName { get; set; }
        public List<SelectListItem> Departments { get; set; } = new();

        //public List<SelectListItem> Roles { get; set; } = new();
    }
}
