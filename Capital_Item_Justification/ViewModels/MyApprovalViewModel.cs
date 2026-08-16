namespace Capital_Item_Justification.ViewModels
{
    public class MyApprovalViewModel
    {
        public long ApprovalId { get; set; }
        public long TransactionId { get; set; }
        public int CIJId { get; set; }
        public string? CIJNumber { get; set; }
        public string? StepName { get; set; }
        public string? ApproverRole { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime? AssignedDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
    }
}
