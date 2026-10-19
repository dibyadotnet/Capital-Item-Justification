namespace Capital_Item_Justification.ViewModels
{
    public class MyApprovalListViewModel
    {
        public List<MyApprovalViewModel> myApprovalViewModels { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
