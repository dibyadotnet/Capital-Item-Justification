namespace Capital_Item_Justification.ViewModels
{
    public class ApprovalRequestDetailsViewModel
    {
        public CIJRequestViewModel CIJRequest { get; set; } = new();
        public List<CIJEquipmentViewModel>  cIJEquipmentViewModels { get; set; } = new();
        public CIJJustificationViewModel cIJJustificationViewModel { get; set; } = new();
        public List<AttachmentViewModel> attachmentViewModels { get; set; } = new();
        public CommitteeCommentViewModel committeeCommentViewModel { get; set; } = new();
    }
}
