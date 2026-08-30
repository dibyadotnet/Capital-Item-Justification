using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capital_Item_Justification.ViewModels
{
    public class ApprovalDetailViewModel
    {
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public long workflowApprovalId { get; set; }
        public string workFlowStepCode { get; set; } = string.Empty;
        public List<ClarificationViewModel> Clarifications { get; set; }= new();
        public bool CanAnswerClarification { get; set; }
        public bool CanRaiseClarification { get; set; }
        public List<string> workflowStepList { get; set; } = new();
        public List<WorkflowHistoryViewModel> workflowHistory { get; set; } = new();
        public int Cijid { get; set; }
        public string? CIJSNumber { get; set; }
        public bool CanReject { get; set; }
        public bool CanQuery { get; set; }
    }
}
