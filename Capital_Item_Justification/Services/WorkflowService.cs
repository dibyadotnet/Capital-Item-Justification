using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Capital_Item_Justification.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _repository;
        public WorkflowService(IWorkflowRepository repository)
        {
            _repository = repository;
        }
        public async Task<bool> SubmitCIJAsync(CIJMainViewModel model, string userId)
        {
           return await _repository.SubmitCIJAsync(model, userId);
        }

        public async Task<List<MyApprovalViewModel>> GetMyApproval(string userId, string roleId)
        {
            return await _repository.GetMyApproval(userId, roleId);
        }
    }
}
