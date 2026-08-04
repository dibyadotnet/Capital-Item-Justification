namespace Capital_Item_Justification.ViewModels
{
    public class DashboardViewModel
    {
        public int CIJId { get; set; }
        public string? CIJNumber { get; set; }
        public DateOnly? RequestDate { get; set; }
        public string? ItemType { get; set; }
        public string? CostCenter { get; set; }
        public decimal? TotalEquipmentCost { get; set; }
        public string? ProjectName { get; set; }
        public string? Status { get; set; }
    }
}
