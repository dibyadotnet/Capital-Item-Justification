namespace Capital_Item_Justification.ViewModels
{
    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
