namespace Capital_Item_Justification.ViewModels
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages =>
            PageSize == 0
                ? 0
                : (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}
