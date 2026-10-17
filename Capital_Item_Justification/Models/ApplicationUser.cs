using Microsoft.AspNetCore.Identity;

namespace Capital_Item_Justification.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public string? EmployeeCode { get; set; }
        public int? DepartmentId { get; set; }
        public int? LocationId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
