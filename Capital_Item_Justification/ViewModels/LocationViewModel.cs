using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class LocationViewModel
    {
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Location Prefix is required.")]
        [StringLength(20)]
        public string Prefix { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location Name is required.")]
        [StringLength(100)]
        public string LocationName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Location Type is required.")]
        [StringLength(50)]
        public string LocationType { get; set; } = string.Empty;

        public List<SelectListItem> LocationTypeList { get; set; } = new();
    }
}
