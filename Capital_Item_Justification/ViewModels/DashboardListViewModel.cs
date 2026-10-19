namespace Capital_Item_Justification.ViewModels
{
    public class DashboardListViewModel
    {
        public List<DashboardViewModel> dashboardViewModels { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
