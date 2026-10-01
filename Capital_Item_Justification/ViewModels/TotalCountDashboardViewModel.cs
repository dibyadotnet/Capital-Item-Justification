namespace Capital_Item_Justification.ViewModels
{
    public class TotalCountDashboardViewModel
    {
        public int TotalRequests { get; set; }
        public int DraftCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int CompletedCount { get; set; }
    }
}
