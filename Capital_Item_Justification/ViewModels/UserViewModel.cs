using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class UserViewModel
    {
        public string? Id { get; set; }

        [Required]
        [Display(Name = "Employee Code")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage ="The Department field is required")]
        public int DepartmentId { get; set; }
        [Required(ErrorMessage = "The Location field is required")]
        public int LocationId { get; set; }

        public bool IsActive { get; set; } = true;
        public string? RoleName { get; set; }

        public string? Password { get; set; }
        public string? DepartmentName { get; set; }
        public string? LocationName { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Locations { get; set; } = new List<SelectListItem>();
    }
}
