using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Repository
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly CIJDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public WorkflowRepository(CIJDbContext context, RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<List<MyApprovalViewModel>> GetMyApprovalAsync(string userId, List<string> roleIds)
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

                                   where app.StatusId == pendingStatusId && roleIds.Contains(app.ApproverRoleId)
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

        public async Task<bool> SubmitCIJAsync(CIJMainViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int cijId = model.CIJRequest.Cijid;
                // 1. Update CIJ main status
                CijRequest? request = new();
                var cijStatus = await _context.CijStatuses.Where(x => x.IsActive == true).ToListAsync();
                int? draftStatusId = cijStatus.Where(x => x.StatusName == "Draft").Select(x => x.StatusId).FirstOrDefault();
                int? submittedStatusId = cijStatus.Where(x => x.StatusName == "Submitted").Select(x => x.StatusId).FirstOrDefault();
                int? pendingApprovalStatusId = cijStatus.Where(x => x.StatusName == "Pending").Select(x => x.StatusId).FirstOrDefault();

                request = await _context.CijRequests.Where(x => x.IsActive == true && x.Cijid == model.CIJRequest.Cijid).FirstOrDefaultAsync();
                if (request == null)
                {
                    throw new Exception("CIJ request not found.");
                }
                request.StatusId = submittedStatusId ?? 2;
                request.ModifiedBy = model.userId;
                request.ModifiedDate = DateTime.Now;
                request.CurrentWorkflowStepId = 1;//workflowid

                var approvalStep = await _context.CijWorkflowSteps.Where(x => x.IsActive && x.StepNo > 1)
                                        .OrderBy(x => x.StepNo).FirstOrDefaultAsync();
                if (approvalStep == null)
                {
                    throw new InvalidOperationException(
                        "No active approval workflow step is configured.");
                }
                var roleDetail = await _roleManager.FindByNameAsync(approvalStep.RoleName);

                if (roleDetail == null)
                {
                    throw new InvalidOperationException(
                        $"Role '{approvalStep.RoleName}' is not configured in AspNetRoles.");
                }
                var actionRoleName = model.userRoles.FirstOrDefault(r => r.Equals(approvalStep.RoleName, StringComparison.OrdinalIgnoreCase));
                if (actionRoleName == null)
                {
                    throw new UnauthorizedAccessException(
                        $"User does not have the required role '{approvalStep.RoleName}'.");
                }
                var actionRole = await _roleManager.FindByNameAsync(actionRoleName);

                if (actionRole == null)
                {
                    throw new InvalidOperationException(
                        $"Role '{actionRoleName}' not found.");
                }
                // 2. Create workflow transaction
                CijWorkflowTransaction trans = new();
                trans.Cijid = cijId;
                trans.WorkflowId = 1;
                trans.CurrentStatusId = pendingApprovalStatusId ?? 3;
                trans.StartDate = DateTime.Now;
                trans.CreatedBy = model.userId;
                trans.CreatedOn = DateTime.Now;
                trans.CurrentStep = approvalStep?.StepName;
                trans.StepId = approvalStep?.WorkflowStepId;

                await _context.CijWorkflowTransactions.AddAsync(trans);
                await _context.SaveChangesAsync();
                long transId = trans.TransactionId;

                // 3. Create workflow approval

                CijWorkflowApproval cijWorkflowApproval = new();
                cijWorkflowApproval.TransactionId = transId;
                cijWorkflowApproval.Cijid = cijId;
                cijWorkflowApproval.StepName = approvalStep?.StepName;
                cijWorkflowApproval.StepId = approvalStep?.WorkflowStepId;
                cijWorkflowApproval.ApproverRole = roleDetail.Name;
                cijWorkflowApproval.ApproverRoleId = roleDetail.Id;
                //cijWorkflowApproval.ApproverUserId = "";//Update the approver UserID who approved the request
                cijWorkflowApproval.StatusId = pendingApprovalStatusId ?? 3;
                cijWorkflowApproval.AssignedDate = DateTime.Now;
                cijWorkflowApproval.ActionDate = DateTime.Now;
                cijWorkflowApproval.CreatedBy = model.userId;
                cijWorkflowApproval.CreatedOn = DateTime.Now;
                await _context.CijWorkflowApprovals.AddAsync(cijWorkflowApproval);
                await _context.SaveChangesAsync();
                long approvalId = cijWorkflowApproval.ApprovalId;

                //4. Create workflow history
                CijWorkflowApprovalHistory cijWorkflowApprovalHistory = new();
                cijWorkflowApprovalHistory.TransactionId = transId;
                cijWorkflowApprovalHistory.Cijid = cijId;
                cijWorkflowApprovalHistory.ApprovalId = approvalId;
                cijWorkflowApprovalHistory.FromStatusId = draftStatusId ?? 1;
                cijWorkflowApprovalHistory.ToStatusId = submittedStatusId ?? 2;
                cijWorkflowApprovalHistory.ApproverUserId = model.userId;
                cijWorkflowApprovalHistory.ApproverRole = actionRole.Id;
                cijWorkflowApprovalHistory.Remarks = "Submitted";
                cijWorkflowApprovalHistory.ActionOn = DateTime.Now;
                cijWorkflowApprovalHistory.CreatedBy = model.userId;
                cijWorkflowApprovalHistory.CreatedOn = DateTime.Now;
                cijWorkflowApprovalHistory.StepId = approvalStep?.WorkflowStepId;
                await _context.CijWorkflowApprovalHistories.AddAsync(cijWorkflowApprovalHistory);
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
        public async Task<ApprovalDetailViewModel?> GetApprovalDetailAsync(int id)
        {
            var approvalDetail = await (from app in _context.CijWorkflowApprovals

                                        join req in _context.CijRequests
                                        on app.Cijid equals req.Cijid

                                        join dept in _context.CijDepartments
                                        on app.DepartmentId equals dept.DepartmentId into deptGroup
                                        from dept in deptGroup.DefaultIfEmpty()

                                        where app.ApprovalId == id
                                        select new ApprovalDetailViewModel()
                                        {
                                            CIJRequest = new CIJRequestViewModel()
                                            {
                                                Cijid = req.Cijid,
                                                CIJSNumber = req.Cijnumber,
                                            },
                                        }).FirstOrDefaultAsync();

            return approvalDetail;
        }
        public async Task<bool?> ApproveRequestAsync(ApproveRejectViewModel vm)
        {
            bool success = false;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Update CIJ main status
                CijRequest? request = new();
                var cijStatus = await _context.CijStatuses.Where(x => x.IsActive == true).ToListAsync();
                int approvedStatusId = cijStatus.Where(x => x.StatusName == "Approved").Select(x => x.StatusId).FirstOrDefault();
                int pendingStatusId = cijStatus.Where(x => x.StatusName == "Pending").Select(x => x.StatusId).FirstOrDefault();

                // 1. Get current approval
                var approval = await _context.CijWorkflowApprovals.FirstOrDefaultAsync(x => x.ApprovalId == vm.workflowApprovalId && x.Cijid == vm.cijId && x.StatusId == pendingStatusId);
                if (approval == null)
                    throw new InvalidOperationException("Pending HOD approval not found.");
                // 2. Get current transaction
                var currentWorkflowTrans = await _context.CijWorkflowTransactions.FirstOrDefaultAsync(x => x.TransactionId == approval.TransactionId && x.Cijid == vm.cijId);
                if (currentWorkflowTrans == null)
                    throw new InvalidOperationException("Workflow transaction not found.");
                // 3. Get current workflow step
                var currentStep = await _context.CijWorkflowSteps.FirstOrDefaultAsync(x => x.WorkflowStepId == currentWorkflowTrans.StepId && x.IsActive && x.WorkflowId == currentWorkflowTrans.WorkflowId);
                if (currentStep == null)
                    throw new InvalidOperationException("Workflow step not found.");

                // 4. Validate HOD role
                var hasRequiredRole = vm.userRoles.Any(x =>
                x.Equals(approval.ApproverRole, StringComparison.OrdinalIgnoreCase));

                if (!hasRequiredRole)
                    throw new UnauthorizedAccessException("You are not authorized to approve this request.");

                // 5. Update workflow approval
                approval.StatusId = approvedStatusId;
                approval.ActionDate = DateTime.UtcNow;
                approval.ModifiedBy = vm.userId;
                approval.ModifiedOn = DateTime.UtcNow;
                approval.Remarks = vm.remarks;
                approval.ApproverUserId = vm.userId;
                // 6. Insert workflow approval history
                var history = new CijWorkflowApprovalHistory
                {
                    TransactionId = currentWorkflowTrans.TransactionId,
                    Cijid = vm.cijId,
                    FromStatusId = pendingStatusId,
                    ApprovalId = approval.ApprovalId,
                    ApproverUserId = vm.userId,
                    ApproverRole = approval.ApproverRole,
                    ToStatusId = approvedStatusId,
                    Remarks = vm.remarks,
                    ActionOn = DateTime.Now,
                    StepId = approval.StepId,
                };
                _context.CijWorkflowApprovalHistories.Add(history);

                // 7. Complete Workflow transaction
                currentWorkflowTrans.CurrentStatusId = approvedStatusId;
                currentWorkflowTrans.CompletionDate = DateTime.UtcNow;
                currentWorkflowTrans.ModifiedBy = vm.userId;
                currentWorkflowTrans.ModifiedOn = DateTime.UtcNow;

                // 8. Get next workflow step
                var nextStep = await _context.CijWorkflowSteps
                    .Where(x => x.IsActive && x.StepNo > currentStep.StepNo)
                    .OrderBy(x => x.StepNo).FirstOrDefaultAsync();
                if (nextStep == null)
                    throw new InvalidOperationException("Next workflow step is not configured.");

                // 9. Create next transaction
                var nextTransaction = new CijWorkflowTransaction
                {
                    Cijid = vm.cijId,
                    WorkflowId = currentWorkflowTrans.WorkflowId,
                    CurrentStatusId = pendingStatusId,
                    StartDate = DateTime.UtcNow,
                    CreatedBy = vm.userId,
                    CreatedOn = DateTime.UtcNow,
                    CurrentStep = nextStep.StepName,
                    StepId = nextStep.WorkflowId,
                };

                _context.CijWorkflowTransactions.Add(nextTransaction);
                await _context.SaveChangesAsync();

                // 10. Create approval for each department selected by HOD
                var departmentHeadRole = await _roleManager.FindByNameAsync(nextStep.RoleName);

                if (departmentHeadRole == null)
                    throw new InvalidOperationException(
                        $"Role '{nextStep.RoleName}' not found.");
                var departmentApprovers = await (from user in _context.Users
                                                 join userRole in _context.UserRoles
                                                 on user.Id equals userRole.UserId
                                                 join role in _context.Roles
                                                 on userRole.RoleId equals role.Id
                                                 where vm.assignedDept.Contains(user.DepartmentId)
                                                 && userRole.RoleId == departmentHeadRole.Id && user.IsActive
                                                 select new
                                                 {
                                                     user.Id,
                                                     user.DepartmentId,
                                                     userRole.RoleId,
                                                     role.Name
                                                 }).ToListAsync();
                foreach (var departmentId in vm.assignedDept.Distinct())
                {
                    var departmentRole = departmentApprovers.FirstOrDefault(x => x.DepartmentId == departmentId);
                    if (departmentRole == null)
                    {
                        throw new InvalidOperationException($"No Department Head found for DepartmentId {departmentId}.");
                    }
                    var cijWorkflowApproval = new CijWorkflowApproval
                    {
                        Cijid = vm.cijId,
                        TransactionId = nextTransaction.TransactionId,
                        StepId = nextStep.WorkflowStepId,
                        DepartmentId = departmentId,
                        ApproverRoleId = departmentRole.RoleId,
                        ApproverRole = departmentRole.Name,
                        StatusId = pendingStatusId,
                        CreatedBy = vm.userId,
                        CreatedOn = DateTime.UtcNow
                    };

                    _context.CijWorkflowApprovals.Add(cijWorkflowApproval);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                success = true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                success = false;
                throw;
            }
            return success;
        }

    }
}
