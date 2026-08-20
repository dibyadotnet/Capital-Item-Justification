namespace Capital_Item_Justification.ViewModels
{
    public class ApproveRejectViewModel
    {
        public long workflowApprovalId { get; set; }
        public int workFlowId { get; set; }
        public int cijId { get; set; }
        public List<int> assignedDept { get; set; } = new();
        public string remarks { get; set; } = string.Empty;
        public string? userId { get; set; } = string.Empty;
        public List<string> userRoles { get; set; } = new();
        public int? userDepartmentId { get; set; }
    }
}
