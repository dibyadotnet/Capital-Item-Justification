using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class RoleViewModel
    {
        public string? Id { get; set; }

        [Required]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;
    }
}
