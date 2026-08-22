namespace Capital_Item_Justification.ViewModels
{
    public class ClarificationViewModel
    {
        public long ClarificationId { get; set; }
        public long ApprovalId { get; set; }
        public string? RaisedBy { get; set; }
        public string? RaisedByRole { get; set; }
        public string ClarificationPoint { get; set; } = string.Empty;
        public DateTime RaisedOn { get; set; }
        public string? AnsweredBy { get; set; }
        public string? Answer { get; set; }
        public DateTime? AnsweredOn { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? TargetRoleId { get; set; } 
        public string? TargetRoleName { get; set; } 
    }
}
