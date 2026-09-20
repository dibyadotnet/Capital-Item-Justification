using Capital_Item_Justification.Models;

namespace Capital_Item_Justification.ViewModels
{
    public class RoleListViewModel
    {
        public List<ApplicationRole> Roles { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
