namespace Capital_Item_Justification.ViewModels
{
    public class LocationListViewModel
    {
        public List<LocationViewModel> locationViewModels { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
