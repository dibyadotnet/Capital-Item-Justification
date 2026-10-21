namespace Capital_Item_Justification.ViewModels
{
    public class ApprovalFilterViewModel
    {
        public int? DepartmentId { get; set; }

        public int? LocationId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int page { get; set; } = 1;

        public int pageSize { get; set; } = 10;
    }
}
