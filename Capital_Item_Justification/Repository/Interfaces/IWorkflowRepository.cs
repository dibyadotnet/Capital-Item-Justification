using Capital_Item_Justification.ViewModels;

namespace Capital_Item_Justification.Repository.Interfaces
{
    public interface IWorkflowRepository
    {
        Task<bool> SubmitCIJAsync(CIJMainViewModel model,string userId);
        Task<List<MyApprovalViewModel>> GetMyApproval(string userId, string roleId);
    }
}
