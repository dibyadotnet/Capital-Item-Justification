namespace Capital_Item_Justification.ViewModels
{
    public class CompletedRequestViewModel
    {
      
        public int CIJId { get; set; }
        public string? CIJNumber { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime? CompletionDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public DateTime? RequestDate { get; set; }
    }
}
