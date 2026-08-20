using Microsoft.AspNetCore.Mvc.Rendering;

namespace Capital_Item_Justification.ViewModels
{
    public class ApprovalDetailViewModel
    {
        public CIJRequestViewModel CIJRequest { get; set; } = new();
        public List<CIJEquipmentViewModel>? Equipments { get; set; } = new();
        public CIJJustificationViewModel? Justification { get; set; } = new();
        public CommitteeCommentViewModel? CommitteeComment { get; set; } = new();
        public List<IFormFile> VendorAttachments { get; set; } = new();
        public List<IFormFile> JustificationAttachment { get; set; } = new();
        public List<AttachmentViewModel> AttachmentVm { get; set; } = new();
        public List<CIJVendorViewModel>? ApprovalHistory { get; set; } = new();
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public long workflowApprovalId { get; set; }
        public string workFlowStepName { get; set; } = string.Empty;
    }
}
