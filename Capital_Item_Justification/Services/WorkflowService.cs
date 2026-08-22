using Capital_Item_Justification.Models;
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
        public async Task<bool> SubmitCIJAsync(CIJMainViewModel model)
        {
           return await _repository.SubmitCIJAsync(model);
        }

        public async Task<List<MyApprovalViewModel>> GetMyApprovalAsync(ApplicationUser user, List<string> roleIds)
        {
            return await _repository.GetMyApprovalAsync(user, roleIds);
        }
        public async Task<ApprovalDetailViewModel?> GetApprovalDetailAsync(int approvalId, int cijId)
        {
            return await _repository.GetApprovalDetailAsync(approvalId, cijId);
        }
        public async Task<bool?> ApproveRequestAsync(ApproveRejectViewModel vm)
        {
            return await _repository.ApproveRequestAsync(vm);
        }
    }
}
