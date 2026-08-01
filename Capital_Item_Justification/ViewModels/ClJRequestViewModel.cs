using Microsoft.AspNetCore.Mvc.Rendering;

namespace Capital_Item_Justification.ViewModels
{
    public class CIJRequestViewModel
    {
        public string? CIJSNumber { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Today;
        public DateTime? RequiredDate { get; set; }
        public string? Priority { get; set; }
        public int? DepartmentId { get; set; }
        public int? CostCenterId { get; set; }
        public int? LocationId { get; set; }
        public string? RequesterName { get; set; }
        public string? Designation { get; set; }
        public string? MobileNo { get; set; }
        public string? Subject { get; set; }
        public int? ItemtypeId { get; set; }
        public List<SelectListItem> ItemTypes { get; set; } = new();
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CostCenters { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Locations { get; set; } = new List<SelectListItem>();
    }
}
