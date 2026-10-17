namespace Capital_Item_Justification.ViewModels
{
    public class UserAssignmentViewModel
    {
        public int AssignmentId { get; set; }

        public string RoleId { get; set; } = "";
        public string RoleName { get; set; } = "";

        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "";

        public int? LocationId { get; set; }
        public string LocationName { get; set; } = "";

        public string ScopeType { get; set; } = "Department";

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
