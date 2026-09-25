namespace Capital_Item_Justification.ViewModels
{
    public class TrackMyRequestListViewModel
    {
        public List<RequestTrackingViewModel> requestTrackings { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
