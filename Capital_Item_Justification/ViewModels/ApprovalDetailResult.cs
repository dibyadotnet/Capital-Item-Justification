namespace Capital_Item_Justification.ViewModels
{
    public class ApprovalDetailResult
    {
        public long workflowApprovalId { get; set; }
        public string workFlowStepCode { get; set; } = string.Empty;
        public int Cijid { get; set; }
        public string? CIJSNumber { get; set; }
        public bool CanReject { get; set; }
        public bool CanQuery { get; set; }
    }
}
