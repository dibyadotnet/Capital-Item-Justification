using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class DepartmentViewModel
    {
        public int DepartmentId { get; set; }
        [StringLength(200)]
        [Required(ErrorMessage = "Department Name is required.")]
        public string DepartmentName { get; set; } = null!;
        public bool? IsActive { get; set; }
    }
}
