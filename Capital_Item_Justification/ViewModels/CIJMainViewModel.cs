using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class CIJMainViewModel
    {
        public CIJRequestViewModel? CIJRequest { get; set; }
        public List<CIJEquipmentViewModel>? Equipments { get; set; }
        public List<CIJVendorViewModel>? Vendors { get; set; }
        public CIJJustificationViewModel? Justification { get; set; }
        public string? EquipmentJson { get; set; }

    }
}
