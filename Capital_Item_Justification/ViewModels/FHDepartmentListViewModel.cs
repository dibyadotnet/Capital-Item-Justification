namespace Capital_Item_Justification.ViewModels
{
    public class FHDepartmentListViewModel
    {
        public List<FunctionHeadDepartmentViewModel> fhDepts { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
