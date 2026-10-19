namespace Capital_Item_Justification.ViewModels
{
    public class RoleBudgetListViewModel
    {
        public List<RoleBudgetLimitViewModel> roleBudgetLimitViewModels { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
