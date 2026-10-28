namespace Capital_Item_Justification.ViewModels
{
    public class CompletedRequestsListViewModel
    {
        public List<CompletedRequestViewModel> completedRequestViewModels { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
