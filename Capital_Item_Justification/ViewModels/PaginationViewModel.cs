namespace Capital_Item_Justification.ViewModels
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }

        public string ActionName { get; set; } = "Index";

        public string ControllerName { get; set; } = "";

        // Used to preserve filters/search
        public Dictionary<string, string?> RouteValues { get; set; } = new();
    }
}
