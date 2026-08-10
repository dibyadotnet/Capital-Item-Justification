using Capital_Item_Justification.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class CIJMainViewModel
    {
        public CIJRequestViewModel? CIJRequest { get; set; }
        public List<CIJEquipmentViewModel>? Equipments { get; set; }
        public List<CIJVendorViewModel>? Vendors { get; set; }
        public CIJJustificationViewModel? Justification { get; set; } = new();
        public CommitteeCommentViewModel? CommitteeComment { get; set; } = new();
        public string? EquipmentJson { get; set; }
        public List<IFormFile> VendorAttachments { get; set; } = new();
        public List<IFormFile> JustificationAttachment { get; set; } = new();
        public List<AttachmentViewModel> AttachmentVm { get; set; } = new();

    }
}
