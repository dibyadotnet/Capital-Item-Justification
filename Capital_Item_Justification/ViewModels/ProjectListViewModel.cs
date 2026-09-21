namespace Capital_Item_Justification.ViewModels
{
    public class ProjectListViewModel
    {
        public List<ProjectViewModel> Projects { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
