using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface IWorkflowRepository
    {
        Task<bool> SubmitCIJAsync(CIJMainViewModel model);
        Task<List<MyApprovalViewModel>> GetMyApprovalAsync(string userId, List<string> roleIds);
        Task<ApprovalDetailViewModel?> GetApprovalDetailAsync(int id);
        Task<bool?> ApproveRequestAsync(ApproveRejectViewModel vm);
    }
}
