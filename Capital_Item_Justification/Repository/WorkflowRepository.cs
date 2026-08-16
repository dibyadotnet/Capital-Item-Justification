using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Repository
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly CIJDbContext _context;
        public WorkflowRepository(CIJDbContext context)
        {
            _context = context;
        }

        public async Task<List<MyApprovalViewModel>> GetMyApproval(string userId, string roleId)
        {
            // Get Pending Status
            var pendingStatusId = await _context.CijStatuses
                .Where(x => x.StatusName == "Pending" && x.IsActive)
                .Select(x => x.StatusId).FirstOrDefaultAsync();

            var approvals = await (from app in _context.CijWorkflowApprovals

                                   join trans in _context.CijWorkflowTransactions
                                   on app.Cijid equals trans.Cijid

                                   join reqs in _context.CijRequests
                                   on trans.Cijid equals reqs.Cijid

                                   join dept in _context.CijDepartments
                                   on app.DepartmentId equals dept.DepartmentId into deptGroup
                                   from dept in deptGroup.DefaultIfEmpty()

                                   where app.StatusId == pendingStatusId && app.ApproverRole == roleId
                                   select new MyApprovalViewModel()
                                   {
                                       ApprovalId = app.ApprovalId,
                                       TransactionId = trans.TransactionId,
                                       CIJId = app.Cijid,
                                       CIJNumber = reqs.Cijnumber,
                                       StepName = app.StepName,
                                       ApproverRole = app.ApproverRole,
                                       DepartmentId = dept.DepartmentId,
                                       DepartmentName = dept != null ? dept.DepartmentName : null,
                                       AssignedDate = app.AssignedDate,
                                       StatusId = app.StatusId,
                                       StatusName = "Pending"
                                   }).ToListAsync();

            return approvals;
        }

        public async Task<bool> SubmitCIJAsync(CIJMainViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Update CIJ main status
                CijRequest? request = new();
                var cijStatus = await _context.CijStatuses.Where(x => x.IsActive == true).ToListAsync();
                int? submittedStatusId = cijStatus.Where(x => x.StatusName == "Submitted").Select(x => x.StatusId).FirstOrDefault();
                int? pendingApprovalStatusId = cijStatus.Where(x => x.StatusName == "Pending").Select(x => x.StatusId).FirstOrDefault();

                request = await _context.CijRequests.Where(x => x.IsActive == true && x.Cijid == model.CIJRequest.Cijid).FirstOrDefaultAsync();
                if (request == null)
                {
                    throw new Exception("CIJ request not found.");
                }
                request.StatusId = submittedStatusId ?? 2;
                request.ModifiedBy = userId;
                request.ModifiedDate = DateTime.Now;

                // 2. Create workflow transaction
                CijWorkflowTransaction trans = new();
                trans.Cijid = model.CIJRequest.Cijid;
                trans.WorkflowId = 1;
                //trans.CurrentApproverRole = "";
                trans.CurrentStatusId = pendingApprovalStatusId ?? 3;
                trans.StartDate = DateTime.Now;
                trans.CreatedBy = userId;
                trans.CreatedOn = DateTime.Now;
                trans.CurrentStep = "HOD Intial Approval";

                await _context.CijWorkflowTransactions.AddAsync(trans);
                await _context.SaveChangesAsync();
                long transId = trans.TransactionId;

                // 3. Create workflow approval
                CijWorkflowApproval cijWorkflowApproval = new();
                cijWorkflowApproval.TransactionId = transId;
                cijWorkflowApproval.Cijid = model.CIJRequest.Cijid;
                cijWorkflowApproval.StepName = "HOD Initial Approval";
                cijWorkflowApproval.ApproverRole = "HOD";
                cijWorkflowApproval.ApproverUserId = "userid";
                cijWorkflowApproval.StatusId = pendingApprovalStatusId ?? 3;
                cijWorkflowApproval.AssignedDate = DateTime.Now;
                cijWorkflowApproval.ActionDate = DateTime.Now;
                cijWorkflowApproval.CreatedBy = "Loggedin Userid";
                cijWorkflowApproval.CreatedOn = DateTime.Now;
                await _context.CijWorkflowApprovals.AddAsync(cijWorkflowApproval);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


    }
}
