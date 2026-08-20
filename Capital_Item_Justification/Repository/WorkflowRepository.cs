using Azure.Core;
using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
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

        public async Task<List<MyApprovalViewModel>> GetMyApprovalAsync(ApplicationUser user, List<string> roleIds)
        {
            // Get Pending Status
            var pendingStatusId = await _context.CijStatuses
                .Where(x => x.StatusName == "Pending" && x.IsActive)
                .Select(x => x.StatusId).FirstOrDefaultAsync();

            var approvals = await (from app in _context.CijWorkflowApprovals

                                   join trans in _context.CijWorkflowTransactions
                                   on app.TransactionId equals trans.TransactionId

                                   join reqs in _context.CijRequests
                                   on trans.Cijid equals reqs.Cijid

                                   join dept in _context.CijDepartments
                                   on app.DepartmentId equals dept.DepartmentId into deptGroup
                                   from dept in deptGroup.DefaultIfEmpty()

                                   where app.StatusId == pendingStatusId
                                   && roleIds.Contains(app.ApproverRoleId)
                                   && (app.DepartmentId == null
                                       || app.DepartmentId == user.DepartmentId)
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
                    throw new InvalidOperationException($"CIJ request with ID {cijId} was not found.");
                }

                int? formStatusId = request.StatusId;
                var firstStep = await _context.CijWorkflowSteps.Where(x => x.IsActive).OrderBy(x => x.StepNo).FirstOrDefaultAsync();

                var approvalStep = await _context.CijWorkflowSteps.Where(x => x.IsActive && x.StepNo > 1)
                                        .OrderBy(x => x.StepNo).FirstOrDefaultAsync();
                if (approvalStep == null)
                {
                    throw new InvalidOperationException(
                        "No active approval workflow step is configured.");
                }
                if (string.IsNullOrWhiteSpace(approvalStep.RoleName))
                {
                    throw new InvalidOperationException(
                        $"Role is not configured for workflow step '{approvalStep.StepName}'.");
                }

                var roleDetail = await _roleManager.FindByNameAsync(approvalStep.RoleName);
                if (roleDetail == null)
                {
                    throw new InvalidOperationException(
                        $"Role '{approvalStep.RoleName}' is not configured in AspNetRoles.");
                }

                //Update CIJ Request
                request.StatusId = submittedStatusId ?? 2;
                request.ModifiedBy = model.userId;
                request.ModifiedDate = DateTime.Now;
                request.WorkflowId = 1;//workflowid

                // 2. Create workflow transaction
                CijWorkflowTransaction trans = new();
                trans.Cijid = cijId;
                trans.WorkflowId = request.WorkflowId;
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
                cijWorkflowApprovalHistory.FromStatusId = formStatusId ?? 1;
                cijWorkflowApprovalHistory.ToStatusId = submittedStatusId ?? 2;
                cijWorkflowApprovalHistory.ApproverUserId = model.userId;
                cijWorkflowApprovalHistory.ApproverRole = roleDetail.Id;
                cijWorkflowApprovalHistory.Remarks = "CIJ request submitted";
                cijWorkflowApprovalHistory.ActionOn = DateTime.Now;
                cijWorkflowApprovalHistory.CreatedBy = model.userId;
                cijWorkflowApprovalHistory.CreatedOn = DateTime.Now;
                cijWorkflowApprovalHistory.StepId = firstStep.WorkflowStepId;  //Requestor workflowstep ID

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

                                        join wfstep in _context.CijWorkflowSteps
                                        on app.StepId equals wfstep.WorkflowStepId

                                        join dept in _context.CijDepartments
                                        on app.DepartmentId equals dept.DepartmentId into deptGroup
                                        from dept in deptGroup.DefaultIfEmpty()

                                        where app.ApprovalId == id
                                        select new ApprovalDetailViewModel()
                                        {
                                            workflowApprovalId = app.ApprovalId,
                                            workFlowStepName = wfstep.StepName,
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
                //CijRequest? request = new();
                var cijStatus = await _context.CijStatuses.Where(x => x.IsActive == true).ToListAsync();
                int approvedStatusId = cijStatus.Where(x => x.StatusName == "Approved").Select(x => x.StatusId).FirstOrDefault();
                int pendingStatusId = cijStatus.Where(x => x.StatusName == "Pending").Select(x => x.StatusId).FirstOrDefault();
                int completedStatusId = cijStatus.Where(x => x.StatusName == "Completed").Select(x => x.StatusId).FirstOrDefault();

                // 1. Get current pending approval
                var approval = await _context.CijWorkflowApprovals.FirstOrDefaultAsync(x => x.ApprovalId == vm.workflowApprovalId && x.Cijid == vm.cijId && x.StatusId == pendingStatusId);
                if (approval == null)
                    throw new InvalidOperationException("Pending HOD approval not found.");

                // 2. Get current workflow transaction
                var currentWorkflowTrans = await _context.CijWorkflowTransactions.FirstOrDefaultAsync(x => x.TransactionId == approval.TransactionId && x.Cijid == vm.cijId);
                if (currentWorkflowTrans == null)
                    throw new InvalidOperationException("Workflow transaction not found.");

                // 3. Get current workflow step
                var currentStep = await _context.CijWorkflowSteps.FirstOrDefaultAsync(
                    x => x.WorkflowStepId == currentWorkflowTrans.StepId
                   && x.IsActive && x.WorkflowId == currentWorkflowTrans.WorkflowId);
                if (currentStep == null)
                    throw new InvalidOperationException("Workflow step not found.");

                // 4. Validate role
                var hasRequiredRole = vm.userRoles.Any(x =>
                x.Equals(approval.ApproverRole, StringComparison.OrdinalIgnoreCase));

                if (!hasRequiredRole)
                    throw new UnauthorizedAccessException("You are not authorized to approve this request.");

                //Validate User Deaprtment configuration

                if (approval.DepartmentId.HasValue)
                {
                    if (!vm.userDepartmentId.HasValue)
                    {
                        throw new UnauthorizedAccessException(
                            "User department is not configured.");
                    }

                    if (approval.DepartmentId.Value != vm.userDepartmentId.Value)
                    {
                        throw new UnauthorizedAccessException(
                            "You are not authorized for this department approval.");
                    }
                }

                // 5. Update current workflow approval
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
                    CreatedBy = vm.userId,
                };
                _context.CijWorkflowApprovalHistories.Add(history);
                await _context.SaveChangesAsync();

                //Check for remaining approvals in SAME STEP IT-approve, Admin -Pending
                bool remainingPendingApprovals = await _context.CijWorkflowApprovals
                .AnyAsync(x => x.Cijid == vm.cijId && x.TransactionId == approval.TransactionId &&
                          x.StepId == approval.StepId && x.StatusId == pendingStatusId);

                //Parallel approval still pending
                if (remainingPendingApprovals)
                {
                    await transaction.CommitAsync();
                    return true;
                }

                // 7. Complete Workflow transaction
                currentWorkflowTrans.CurrentStatusId = approvedStatusId;
                currentWorkflowTrans.CompletionDate = DateTime.UtcNow;
                currentWorkflowTrans.ModifiedBy = vm.userId;
                currentWorkflowTrans.ModifiedOn = DateTime.UtcNow;

                //Get CIJ Request 
                var cijRequest = await _context.CijRequests.FirstOrDefaultAsync(x => x.Cijid == vm.cijId && x.IsActive == true);
                if (cijRequest == null)
                {
                    throw new InvalidOperationException("CIJ request not found.");
                }

                CijWorkflowStep? nextStep = await GetWorkflowNextStep(currentStep, currentWorkflowTrans, cijRequest, approval);
                if (nextStep == null)
                    throw new InvalidOperationException("Next workflow step is not configured.");

                // Workflow is completely finished.
                if (nextStep?.StepName == "Completed")
                {
                    if (cijRequest != null)
                    {
                        cijRequest.StatusId = completedStatusId;
                        cijRequest.ModifiedBy = vm.userId;
                        cijRequest.ModifiedDate = DateTime.Now;
                    }
                    //Update Workflow Transction
                    currentWorkflowTrans.CurrentStatusId = completedStatusId;
                    currentWorkflowTrans.CompletionDate = DateTime.UtcNow;
                    currentWorkflowTrans.ModifiedBy = vm.userId;
                    currentWorkflowTrans.ModifiedOn = DateTime.UtcNow;

                    //Update Workflow Approval
                    approval.StatusId = completedStatusId;
                    approval.ActionDate = DateTime.UtcNow;
                    approval.ModifiedBy = vm.userId;
                    approval.ModifiedOn = DateTime.UtcNow;

                    var completionHistory = new CijWorkflowApprovalHistory
                    {
                        TransactionId = currentWorkflowTrans.TransactionId,
                        Cijid = vm.cijId,
                        ApprovalId = approval.ApprovalId,
                        ApproverUserId = vm.userId,
                        ApproverRole = approval.ApproverRole,
                        FromStatusId = approvedStatusId,
                        ToStatusId = completedStatusId,
                        Remarks = "Workflow completed.",
                        ActionOn = DateTime.UtcNow,
                        StepId = approval.StepId,
                        CreatedBy = vm.userId,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.CijWorkflowApprovalHistories.Add(completionHistory);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }

                // 9. Create next workflow transaction
                var nextTransaction = new CijWorkflowTransaction
                {
                    Cijid = vm.cijId,
                    WorkflowId = currentWorkflowTrans.WorkflowId,
                    CurrentStatusId = pendingStatusId,
                    StartDate = DateTime.UtcNow,
                    CreatedBy = vm.userId,
                    CreatedOn = DateTime.UtcNow,
                    CurrentStep = nextStep.StepName,
                    StepId = nextStep.WorkflowStepId,
                };

                _context.CijWorkflowTransactions.Add(nextTransaction);
                await _context.SaveChangesAsync();
                if (currentStep.RoleName.Equals("HOD", StringComparison.OrdinalIgnoreCase) && currentStep.StepName.Equals("HOD Initial Approval", StringComparison.OrdinalIgnoreCase))
                {
                    if (vm.assignedDept == null || !vm.assignedDept.Any())
                    {
                        throw new InvalidOperationException("No departments were selected by HOD.");
                    }
                    // 10. Create approval for each department selected by HOD
                    var departmentHeadRole = await _roleManager.FindByNameAsync(nextStep.RoleName);

                    if (departmentHeadRole == null)
                        throw new InvalidOperationException($"Role '{nextStep.RoleName}' not found.");

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
                        var departmentApproverRole = departmentApprovers.FirstOrDefault(x => x.DepartmentId == departmentId);
                        if (departmentApproverRole == null)
                        {
                            throw new InvalidOperationException($"No Department Head found for DepartmentId {departmentId}.");
                        }
                        var cijWorkflowApproval = new CijWorkflowApproval
                        {
                            Cijid = vm.cijId,
                            TransactionId = nextTransaction.TransactionId,
                            StepId = nextStep.WorkflowStepId,
                            DepartmentId = departmentId,
                            ApproverRoleId = departmentApproverRole.RoleId,
                            ApproverRole = departmentApproverRole.Name,
                            StatusId = pendingStatusId,
                            CreatedBy = vm.userId,
                            CreatedOn = DateTime.UtcNow,
                            StepName = nextStep.StepName,
                            AssignedDate = DateTime.UtcNow,
                        };

                        _context.CijWorkflowApprovals.Add(cijWorkflowApproval);
                    }
                }
                else
                {
                    //Normal single-role approval
                    //var nextRole = await _roleManager.FindByNameAsync(nextStep.RoleName);
                    var nextRole = await _context.Roles.FirstOrDefaultAsync(r =>r.Name != null &&
                                r.Name.Trim().ToLower() == nextStep.RoleName.Trim().ToLower());

                    if (nextRole == null)
                    {
                        throw new InvalidOperationException($"Role '{nextStep.RoleName}' not found.");
                    }

                    var nextApproval = new CijWorkflowApproval
                    {
                        Cijid = vm.cijId,
                        TransactionId = nextTransaction.TransactionId,
                        StepId = nextStep.WorkflowStepId,
                        DepartmentId = null,
                        ApproverRoleId = nextRole.Id,
                        ApproverRole = nextRole.Name,
                        StatusId = pendingStatusId,
                        StepName = nextStep.StepName,
                        AssignedDate = DateTime.UtcNow,
                        CreatedBy = vm.userId,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.CijWorkflowApprovals.Add(nextApproval);
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

        private async Task<CijWorkflowStep?> GetWorkflowNextStep(CijWorkflowStep currentStep, CijWorkflowTransaction currentTransaction, CijRequest cijRequest, CijWorkflowApproval cijWorkflowApproval)
        {
            CijWorkflowStep? nextStep = new CijWorkflowStep();

            if (currentStep.StepName.Equals("HOD Initial Approval", StringComparison.OrdinalIgnoreCase))
            {
                nextStep = await _context.CijWorkflowSteps.Where(x => x.WorkflowId == currentTransaction.WorkflowId
                && x.IsActive && x.StepName == "Department Head Approval").OrderBy(x => x.StepNo).FirstOrDefaultAsync();

                if (nextStep == null)
                {
                    throw new InvalidOperationException("Department Head Approval step is not configured.");
                }
            }
            else if (currentStep.StepName.Equals("Department Head Approval", StringComparison.OrdinalIgnoreCase))
            {
                var itemType = await _context.CijItemTypes.FirstOrDefaultAsync(x => x.ItemTypeId == cijRequest.ItemTypeId);
                if (itemType == null)
                {
                    throw new InvalidOperationException("CIJ item type not found.");
                }
                if (itemType.ItemTypeName.Equals("Medical", StringComparison.OrdinalIgnoreCase))
                {
                    nextStep = await _context.CijWorkflowSteps.Where(x =>
                            x.WorkflowId == currentTransaction.WorkflowId &&
                            x.IsActive && x.StepName == "Purchase Committee Approval")
                            .OrderBy(x => x.StepNo).FirstOrDefaultAsync();
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Purchase Committee Approval step is not configured.");
                    }
                }
                else
                {
                    nextStep = await _context.CijWorkflowSteps
                        .Where(x => x.WorkflowId == currentTransaction.WorkflowId &&
                                x.IsActive && x.StepName == "HOD Final Approval")
                                .OrderBy(x => x.StepNo).FirstOrDefaultAsync();
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("HOD Final Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepName.Equals("Purchase Committee Approval", StringComparison.OrdinalIgnoreCase))
            {
                nextStep = await _context.CijWorkflowSteps
                    .Where(x => x.WorkflowId == currentTransaction.WorkflowId &&
                        x.IsActive && x.StepName == "HOD Final Approval")
                    .OrderBy(x => x.StepNo).FirstOrDefaultAsync();

                if (nextStep == null)
                {
                    throw new InvalidOperationException(
                        "Final HOD Approval step is not configured.");
                }
            }
            else if (currentStep.StepName.Equals("HOD Final Approval", StringComparison.OrdinalIgnoreCase))
            {
                if (cijRequest.BudgetAvailable == "Yes")
                {
                    nextStep = await _context.CijWorkflowSteps
                       .Where(x => x.WorkflowId == currentTransaction.WorkflowId &&
                       x.IsActive && x.StepName == "Finance Approval")
                       .OrderBy(x => x.StepNo).FirstOrDefaultAsync();

                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Finance Approval step is not configured.");
                    }
                }
                else
                {
                    nextStep = await _context.CijWorkflowSteps
                            .Where(x => x.WorkflowId == currentTransaction.WorkflowId &&
                            x.IsActive && x.StepName == "COO Approval")
                            .OrderBy(x => x.StepNo).FirstOrDefaultAsync();

                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("COO Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepName.Equals("COO Approval", StringComparison.OrdinalIgnoreCase))
            {
                nextStep = await _context.CijWorkflowSteps
                   .Where(x => x.WorkflowId == currentTransaction.WorkflowId &&
                   x.IsActive && x.StepName == "Finance Approval")
                   .OrderBy(x => x.StepNo).FirstOrDefaultAsync();

                if (nextStep == null)
                {
                    throw new InvalidOperationException("Finance Approval step is not configured.");
                }
            }
            else if (currentStep.StepName.Equals("Finance Approval", StringComparison.OrdinalIgnoreCase))
            {
                var hodRoleId= await _context.Roles.Where(x => x.IsActive == true && x.Name == "HOD").Select(a=>a.Id).FirstOrDefaultAsync();
                if (hodRoleId == null)
                {
                    throw new InvalidOperationException("HOD Role is not configured.");
                }
                var budgetLimit = await _context.CijRoleBudgetLimits.Where(x => x.IsActive == true && x.RoleId == hodRoleId).FirstOrDefaultAsync();
                if (budgetLimit == null)
                {
                    throw new InvalidOperationException("Budget Limit is not configured for this Role.");
                }
                if (budgetLimit?.BudgetLimit >= cijRequest.TotalEquipmentCost)
                {
                    nextStep = await _context.CijWorkflowSteps
                            .Where(x => x.WorkflowId == currentTransaction.WorkflowId &&
                            x.IsActive && x.StepName == "Completed")
                            .OrderBy(x => x.StepNo).FirstOrDefaultAsync();

                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Completed step is not configured.");
                    }
                }
                else
                {
                    nextStep = await _context.CijWorkflowSteps
                            .Where(x => x.WorkflowId == currentTransaction.WorkflowId &&
                            x.IsActive && x.StepName == "MD Approval")
                            .OrderBy(x => x.StepNo).FirstOrDefaultAsync();

                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("MD Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepName.Equals("MD Approval", StringComparison.OrdinalIgnoreCase))
            {
                nextStep = await _context.CijWorkflowSteps
                            .Where(x => x.WorkflowId == currentTransaction.WorkflowId &&
                            x.IsActive && x.StepName == "CEO Approval")
                            .OrderBy(x => x.StepNo).FirstOrDefaultAsync();

                if (nextStep == null)
                {
                    throw new InvalidOperationException("CEO Approval step is not configured.");
                }
            }
            else if (currentStep.StepName.Equals("CEO Approval", StringComparison.OrdinalIgnoreCase))
            {
                nextStep = await _context.CijWorkflowSteps
                            .Where(x => x.WorkflowId == currentTransaction.WorkflowId &&
                            x.IsActive && x.StepName == "Completed")
                            .OrderBy(x => x.StepNo).FirstOrDefaultAsync();

                if (nextStep == null)
                {
                    throw new InvalidOperationException("CEO Approval step is not configured.");
                }
            }
            else
            {
                throw new InvalidOperationException($"Workflow step '{currentStep.StepName}' is not configured.");
            }

            return nextStep;
        }

    }
}
