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
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public long workflowApprovalId { get; set; }
        public string workFlowStepName { get; set; } = string.Empty;
        public List<ClarificationViewModel> Clarifications { get; set; }= new();
        public bool CanAnswerClarification { get; set; }
        public bool CanRaiseClarification { get; set; }
        public List<string> workflowStepList { get; set; } = new();
        public List<WorkflowHistoryViewModel> workflowHistory { get; set; } = new();
    }
}
