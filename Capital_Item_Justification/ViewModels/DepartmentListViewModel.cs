using System.Data;

namespace Capital_Item_Justification.ViewModels
{
    public class DepartmentListViewModel
    {
        public List<DepartmentViewModel> Departments { get; set; } = new ();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
