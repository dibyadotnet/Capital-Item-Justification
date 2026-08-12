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

        public int? DepartmentId { get; set; }

        public int? LocationId { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Password { get; set; }
    }
}
