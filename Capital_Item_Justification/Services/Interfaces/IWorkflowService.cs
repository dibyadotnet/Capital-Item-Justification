using Capital_Item_Justification.Models;
using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Services.Interfaces
{
    public interface IWorkflowService
    {
        Task<bool> SubmitCIJAsync(CIJMainViewModel model);
        Task<List<MyApprovalViewModel>> GetMyApprovalAsync(ApplicationUser user, List<string> roleIds);
        Task<ApprovalDetailViewModel?> GetApprovalDetailAsync(int id);
        Task<bool?> ApproveRequestAsync(ApproveRejectViewModel vm);
    }
}
