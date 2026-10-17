using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class SelectRoleViewModel
    {
        [Required(ErrorMessage = "Please select a role.")]
        public int? AssignmentId { get; set; }

        public List<RoleAssignmentOptionViewModel> Roles { get; set; }
            = new();
    }
    public class RoleAssignmentOptionViewModel
    {
        public int AssignmentId { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string ScopeType { get; set; } = string.Empty;
    }
}

