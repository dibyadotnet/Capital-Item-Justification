using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class CIJFormViewModel
    {
        public string? CIJSNumber { get; set; }

        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; } = DateTime.Today;

        [Display(Name = "Required Date")]
        public DateTime? RequiredDate { get; set; }
        public string? Priority { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }

        [Display(Name = "Location")]
        public int? LocationId { get; set; }

        [Display(Name = "Requester")]
        public string? RequesterName { get; set; }

        public string? Designation { get; set; }

        [Display(Name = "Mobile No")]
        public string? MobileNo { get; set; }

        public string? Subject { get; set; }

        public IEnumerable<SelectListItem> Departments { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> CostCenters { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> Locations { get; set; }
            = new List<SelectListItem>();
    }
}
