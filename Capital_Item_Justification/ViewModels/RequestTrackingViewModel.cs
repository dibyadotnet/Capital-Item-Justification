namespace Capital_Item_Justification.ViewModels
{
    public class RequestTrackingViewModel
    {
        public int CijId { get; set; }
        public string? CIJNumber { get; set; }
        public long WorkflowApprovalId { get; set; }
        public string? ApproverRole { get; set; }
        public string? CurrentStatus { get; set; }
        public DateTime? PendingSince { get; set; }
        public string? WorkFlowStepCode { get; set; }
    }
}
