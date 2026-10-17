using System.ComponentModel.DataAnnotations;
namespace Capital_Item_Justification.ViewModels
{
    public class SaveUserAssignmentRequest
    {
        public int AssignmentId { get; set; }
        [Required]
        public string UserId { get; set; } = "";
        [Required]
        public string RoleId { get; set; } = "";
        [Required]
        public int? DepartmentId { get; set; }
        [Required]
        public int? LocationId { get; set; }
    }
}
