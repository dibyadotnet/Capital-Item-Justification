using Capital_Item_Justification.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Capital_Item_Justification.ViewModels
{
    public class CIJMainViewModel
    {
        public CIJRequestViewModel CIJRequest { get; set; } = new();
        public List<CIJEquipmentViewModel>? Equipments { get; set; } = new();
        public List<CIJVendorViewModel>? Vendors { get; set; } = new();
        public CIJJustificationViewModel? Justification { get; set; } = new();
        public CommitteeCommentViewModel? CommitteeComment { get; set; } = new();
        public string? EquipmentJson { get; set; }
        public List<IFormFile> VendorAttachments { get; set; } = new();
        public List<IFormFile> JustificationAttachment { get; set; } = new();
        public List<AttachmentViewModel> AttachmentVm { get; set; } = new();
        public List<CIJVendorViewModel>? ApprovalHistory { get; set; } = new();

        public string? userId { get; set; } = string.Empty;
        public string? roleId { get; set; } = string.Empty;
        public string? roleName { get; set; } = string.Empty;
        public List<string> userRoles { get; set; } = new();
        public int? userDepartmentId { get; set; }

    }
}
