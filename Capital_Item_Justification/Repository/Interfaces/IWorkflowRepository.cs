using Capital_Item_Justification.Models;
using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface IWorkflowRepository
    {
        Task<bool> SubmitCIJAsync(CIJMainViewModel model);
        Task<List<MyApprovalViewModel>> GetMyApprovalAsync(ApplicationUser user, List<string> roleIds);
        Task<ApprovalDetailViewModel?> GetApprovalDetailAsync(int approvalId, int cijId);
        Task<bool?> ApproveRequestAsync(ApproveRejectViewModel vm);
    }
}
