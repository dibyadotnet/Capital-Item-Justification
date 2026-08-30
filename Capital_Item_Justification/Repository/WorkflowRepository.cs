using Azure.Core;
using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.Linq;
using System.Net.NetworkInformation;

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
            var roleIdsString = string.Join(",", roleIds);

            var departmentParam = new SqlParameter(
                "@UserDepartmentId",
                user.DepartmentId);

            var roleIdsParam = new SqlParameter(
                "@RoleIds",
                roleIdsString);

            var result = await _context.Set<MyApprovalViewModel>()
                .FromSqlRaw(
                    "EXEC dbo.SP_GetMyApprovals @UserDepartmentId, @RoleIds",
                    departmentParam,
                    roleIdsParam)
                .AsNoTracking()
                .ToListAsync();

            var purchaseRole = await (from role in _context.Roles
                                      join roleId in roleIds
                                     on role.Id equals roleId
                                      where role.Name == "Purchase"
                                      select role).FirstOrDefaultAsync();

            if (purchaseRole != null)
            {
                return result;
            }
            else
            {
                int pendingApprovalStatusId = _context.CijStatuses.Where(x => x.StatusName == "Pending" && x.IsActive == true).Select(x => x.StatusId).FirstOrDefault();
                if (pendingApprovalStatusId == 0)
                    throw new InvalidOperationException("Pending status is not configured.");
                result = result.Where(a => a.StatusId == pendingApprovalStatusId).ToList();
            }
            return result;
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
                int draftStatusId = cijStatus.Where(x => x.StatusName == "Draft").Select(x => x.StatusId).FirstOrDefault();
                if (draftStatusId == 0)
                    throw new InvalidOperationException("Draft status is not configured.");

                int submittedStatusId = cijStatus.Where(x => x.StatusName == "Submitted").Select(x => x.StatusId).FirstOrDefault();
                if (submittedStatusId == 0)
                    throw new InvalidOperationException("Submitted status is not configured.");

                int pendingApprovalStatusId = cijStatus.Where(x => x.StatusName == "Pending").Select(x => x.StatusId).FirstOrDefault();
                if (pendingApprovalStatusId == 0)
                    throw new InvalidOperationException("Pending status is not configured.");

                request = await _context.CijRequests.Where(x => x.IsActive == true && x.Cijid == model.CIJRequest.Cijid).FirstOrDefaultAsync();
                if (request == null)
                {
                    throw new InvalidOperationException($"CIJ request with ID {cijId} was not found.");
                }

                int? formStatusId = request.StatusId;
                //Get WorkFlow ID
                int workflowId = 0;
                string workflowCode = "0";
                var locartiontype = await _context.CijLocations.Where(a => a.LocationId == request.LocationId && a.IsActive).Select(a => a.LocationType).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(locartiontype))
                {
                    throw new InvalidOperationException($"Location type is not found.");
                }
                var costCenter = await _context.CijCostCenters.Where(a => a.IsActive == true && a.CostCenterId == request.CostCenterId).Select(a => a.CostCenterName).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(costCenter))
                {
                    throw new InvalidOperationException($"costCenter is not found.");
                }
                string? fundingType = string.Empty;
                if (costCenter == "Project")
                {
                    fundingType = await _context.CijBudgetTypes.Where(a => a.IsActive == true && a.BudgetTypeId == request.BudgetTypeId).Select(a => a.BudgetTypeName).FirstOrDefaultAsync();
                    if (string.IsNullOrWhiteSpace(fundingType))
                    {
                        throw new InvalidOperationException($"Funding Type is not found.");
                    }
                }

                if (locartiontype == "Primary")
                {
                    if (costCenter == "SCEH" || (costCenter == "Project" && fundingType == "Partially Funded"))
                    {
                        workflowId = 1;
                        workflowCode = "WF001";
                    }
                }
                if (locartiontype == "Secondary" && costCenter == "SCEH")
                {
                    workflowId = 2;
                    workflowCode = "WF002";
                }
                if (fundingType == "Fully Funded")
                {
                    workflowId = 3;
                    workflowCode = "WF003";
                }
                //------
                var firstStep = await _context.CijWorkflowSteps.Where(x => x.IsActive && x.WorkflowId == workflowId).OrderBy(x => x.StepNo).FirstOrDefaultAsync();
                if (firstStep == null)
                    throw new InvalidOperationException($"Requestor step is not configured for {workflowCode}");

                //Get Next Workflow Step
                CijWorkflowStep? approvalStep = null;
                if (workflowId == 1)
                    approvalStep = await GetWorkflowNextStep(firstStep, request, workflowId);
                if (workflowId == 2)
                    approvalStep = await GetSecondWorkflowNextStep(firstStep, request, workflowId);
                if (workflowId == 3)
                    approvalStep = await GetThirdWorkflowNextStep(firstStep, request, workflowId);

                if (approvalStep == null)
                    throw new InvalidOperationException($"Next workflow step is not configured for {workflowCode}");

                //var approvalStep = await _context.CijWorkflowSteps.Where(x => x.IsActive && x.StepNo > 1)
                //                            .OrderBy(x => x.StepNo).FirstOrDefaultAsync();
                if (approvalStep == null)
                {
                    throw new InvalidOperationException(
                        "No active approval workflow step is configured.");
                }
                if (string.IsNullOrWhiteSpace(approvalStep.RoleName))
                {
                    throw new InvalidOperationException(
                        $"Role is not configured for workflow step '{approvalStep.StepName}' for {workflowCode}");
                }
                var roleDetail = await _context.Roles.FirstOrDefaultAsync(r => r.Name != null && r.Name.ToLower() == approvalStep.RoleName.ToLower() && r.IsActive);
                //var roleDetail = await _roleManager.FindByNameAsync(approvalStep.RoleName);
                if (roleDetail == null)
                {
                    throw new InvalidOperationException(
                        $"Role '{approvalStep.RoleName}' is not configured in AspNetRoles.");
                }

                //Get Requestor Role
                var requesterRole = await (from ur in _context.UserRoles
                                           join role in _context.Roles
                                            on ur.RoleId equals role.Id
                                           where ur.UserId == model.userId
                                           where role.Name == "Requester"
                                           select new
                                           {
                                               RoleId = role.Id,
                                               RoleName = role.Name
                                           }).FirstOrDefaultAsync();


                //Update CIJ Request
                request.StatusId = submittedStatusId;
                request.ModifiedBy = model.userId;
                request.ModifiedDate = DateTime.Now;
                request.WorkflowId = workflowId;

                // 2. Create workflow transaction
                CijWorkflowTransaction trans = new();
                trans.Cijid = cijId;
                trans.WorkflowId = workflowId;
                trans.CurrentStatusId = pendingApprovalStatusId;
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
                cijWorkflowApproval.StatusId = pendingApprovalStatusId;
                cijWorkflowApproval.AssignedDate = DateTime.Now;
                cijWorkflowApproval.ActionDate = DateTime.Now;
                cijWorkflowApproval.CreatedBy = model.userId;
                cijWorkflowApproval.CreatedOn = DateTime.Now;
                cijWorkflowApproval.DepartmentId = request.RequestDepartmentId;
                await _context.CijWorkflowApprovals.AddAsync(cijWorkflowApproval);
                await _context.SaveChangesAsync();
                long approvalId = cijWorkflowApproval.ApprovalId;

                //4. Create workflow history
                CijWorkflowApprovalHistory cijWorkflowApprovalHistory = new();
                cijWorkflowApprovalHistory.TransactionId = null;
                cijWorkflowApprovalHistory.Cijid = cijId;
                //cijWorkflowApprovalHistory.ApprovalId = 0;
                cijWorkflowApprovalHistory.FromStatusId = formStatusId ?? 1;
                cijWorkflowApprovalHistory.ToStatusId = submittedStatusId;
                cijWorkflowApprovalHistory.ApproverUserId = model.userId;
                cijWorkflowApprovalHistory.ApproverRole = requesterRole.RoleName;
                cijWorkflowApprovalHistory.ApproverRoleId = requesterRole.RoleId;
                cijWorkflowApprovalHistory.Remarks = "CIJ request submitted";
                cijWorkflowApprovalHistory.ActionOn = DateTime.Now;
                cijWorkflowApprovalHistory.CreatedBy = model.userId;
                cijWorkflowApprovalHistory.CreatedOn = DateTime.Now;
                cijWorkflowApprovalHistory.StepId = firstStep.WorkflowStepId;  //Requestor workflowstep ID
                cijWorkflowApprovalHistory.DepartmentId = request.RequestDepartmentId;

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
        public async Task<ApprovalDetailViewModel?> GetApprovalDetailAsync(int approvalId, int cijId)
        {
            // 1. Get approval details
            var approvalDetail = await GetApprovalDetailFromSPAsync(approvalId, cijId);
            if (approvalDetail == null)
                return null;
            // 2. Get clarification history
            var clarificationHistory = await GetClarificationHistoryFromSPAsync(approvalId, cijId);
            // 3. Get approval/workflow history
            var history = await GetWorkflowHistoryFromSPAsync(cijId);

            // Assign clarification history
            if (clarificationHistory.Count > 0)
            {
                approvalDetail.Clarifications = clarificationHistory;
            }
            // Assign approval history
            if (history.Count > 0)
            {
                approvalDetail.workflowHistory = history;
            }
            return approvalDetail;
        }
        private async Task<ApprovalDetailViewModel?> GetApprovalDetailFromSPAsync(int approvalId, int cijId)
        {
            var approvalDetailResults = await _context
                .Set<ApprovalDetailResult>()
                .FromSqlInterpolated($@"
            EXEC dbo.SP_GetApprovalDetail
                @ApprovalId = {approvalId},
                @CIJId = {cijId}")
                .AsNoTracking()
                .ToListAsync();

            var approvalDetail = approvalDetailResults.FirstOrDefault();
            if (approvalDetail == null)
                return null;
            ApprovalDetailViewModel? vm = null;

            if (approvalDetail != null)
            {
                vm = new ApprovalDetailViewModel
                {
                    workflowApprovalId = approvalDetail.workflowApprovalId,
                    workFlowStepCode = approvalDetail.workFlowStepCode,
                    Cijid = approvalDetail.Cijid,
                    CIJSNumber = approvalDetail.CIJSNumber,
                    CanReject = approvalDetail.CanReject,
                    CanQuery = approvalDetail.CanQuery
                };
            }
            return vm;
        }
        private async Task<List<ClarificationViewModel>> GetClarificationHistoryFromSPAsync(int approvalId, int cijId)
        {
            var result = await _context
                .Set<ClarificationViewModel>()
                .FromSqlInterpolated($@"
            EXEC dbo.SP_GetClarificationHistory
                @ApprovalId = {approvalId},
                @CIJId = {cijId}")
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        private async Task<List<WorkflowHistoryViewModel>> GetWorkflowHistoryFromSPAsync(int cijId)
        {
            var result = await _context
                .Set<WorkflowHistoryViewModel>()
                .FromSqlInterpolated($@"
            EXEC dbo.SP_GetWorkflowHistory
                @CIJId = {cijId}")
                .AsNoTracking()
                .ToListAsync();

            return result;
        }
        public async Task<bool?> ApproveRequestAsync(ApproveRejectViewModel vm)
        {
            if (string.Equals(vm.Action, "Query", StringComparison.OrdinalIgnoreCase))
            {
                return await RaiseQueryAsync(vm);
            }
            if (string.Equals(vm.Action, "Answer", StringComparison.OrdinalIgnoreCase))
            {
                return await AnswerQueryAsync(vm);
            }
            if (string.Equals(vm.Action, "Approve", StringComparison.OrdinalIgnoreCase))
            {
                return await ApproveWorkflowAsync(vm);
            }
            if (string.Equals(vm.Action, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                return await RejectWorkflowAsync(vm);
            }

            throw new InvalidOperationException("Invalid workflow action.");
        }
        private async Task<bool?> ApproveWorkflowAsync(ApproveRejectViewModel vm)
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
                    throw new InvalidOperationException("Pending approval not found.");

                // 2. Get current workflow transaction
                var currentWorkflowTrans = await _context.CijWorkflowTransactions.FirstOrDefaultAsync(x => x.TransactionId == approval.TransactionId && x.Cijid == vm.cijId && x.IsActive == true);
                if (currentWorkflowTrans == null)
                    throw new InvalidOperationException("Workflow transaction not found.");

                // 3. Get current workflow step
                var currentStep = await _context.CijWorkflowSteps.FirstOrDefaultAsync(
                    x => x.WorkflowStepId == currentWorkflowTrans.StepId
                   && x.IsActive && x.WorkflowId == currentWorkflowTrans.WorkflowId);
                if (currentStep == null)
                    throw new InvalidOperationException("Workflow step not found.");

                // 4. Validate role check Department
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
                    ApproverRoleId = approval.ApproverRoleId,
                    ToStatusId = approvedStatusId,
                    Remarks = vm.remarks,
                    ActionOn = DateTime.Now,
                    StepId = approval.StepId,
                    CreatedBy = vm.userId,
                    DepartmentId = approval.DepartmentId
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

                //Get Next Workflow Step
                CijWorkflowStep? nextStep = await GetWorkflowNextStep(currentStep, cijRequest, cijRequest.WorkflowId ?? 0);
                if (nextStep == null)
                    throw new InvalidOperationException("Next workflow step is not configured.");

                // Workflow is completely finished.
                if (nextStep?.IsFinalStep == true)
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
                        ApproverRoleId = approval.ApproverRoleId,
                        FromStatusId = approvedStatusId,
                        ToStatusId = completedStatusId,
                        Remarks = "Workflow completed.",
                        ActionOn = DateTime.UtcNow,
                        StepId = approval.StepId,
                        CreatedBy = vm.userId,
                        CreatedOn = DateTime.UtcNow,
                        DepartmentId = approval.DepartmentId
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

                if (currentStep.StepCode.Equals("HOD_Initial", StringComparison.OrdinalIgnoreCase) || currentStep.StepCode.Equals("Function_Head_Initial", StringComparison.OrdinalIgnoreCase))
                {
                    if (vm.assignedDept == null || !vm.assignedDept.Any())
                    {
                        throw new InvalidOperationException("No departments were selected by HOD/Function Head");
                    }
                    // 10. Create approval for each department selected by HOD initial and Function head initial
                    var techinicalAssessementRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name != null &&
                                r.Name.Trim().ToLower() == nextStep.RoleName.Trim().ToLower());
                    if (techinicalAssessementRole == null)
                        throw new InvalidOperationException($"Role '{nextStep.RoleName}' not found.");

                    //var functionHeadApprovers = await (from user in _context.Users
                    //                                   join userRole in _context.UserRoles
                    //                                   on user.Id equals userRole.UserId
                    //                                   join role in _context.Roles
                    //                                   on userRole.RoleId equals role.Id
                    //                                   where vm.assignedDept.Contains(user.DepartmentId)
                    //                                   && userRole.RoleId == techinicalAssessementRole.Id && user.IsActive
                    //                                   select new
                    //                                   {
                    //                                       user.Id,
                    //                                       user.DepartmentId,
                    //                                       userRole.RoleId,
                    //                                       role.Name
                    //                                   }).ToListAsync();
                    foreach (var departmentId in vm.assignedDept.Distinct())
                    {
                        //var functionHeadApproverRole = functionHeadApprovers.FirstOrDefault(x => x.DepartmentId == departmentId);
                        //if (functionHeadApproverRole == null)
                        //{
                        //    throw new InvalidOperationException($"No Function Head found for DepartmentId {departmentId}.");
                        //}
                        var cijWorkflowApproval = new CijWorkflowApproval
                        {
                            Cijid = vm.cijId,
                            TransactionId = nextTransaction.TransactionId,
                            StepId = nextStep.WorkflowStepId,
                            DepartmentId = departmentId,
                            ApproverRoleId = techinicalAssessementRole.Id,
                            ApproverRole = techinicalAssessementRole.Name,
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
                    var nextRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name != null &&
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
                        DepartmentId = nextStep.DepartmentId ?? cijRequest.RequestDepartmentId,
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
        private async Task<bool> RaiseQueryAsync(ApproveRejectViewModel vm)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cijStatus = await _context.CijStatuses.Where(x => x.IsActive).ToListAsync();
                int queryStatusId = cijStatus.Where(x => x.StatusName == "Query").Select(x => x.StatusId).FirstOrDefault();
                int pendingStatusId = cijStatus.Where(x => x.StatusName == "Pending").Select(x => x.StatusId).FirstOrDefault();
                int approvedStatusId = cijStatus.Where(x => x.StatusName == "Approved").Select(x => x.StatusId).FirstOrDefault();
                if (queryStatusId == 0)
                    throw new InvalidOperationException("Query status is not configured.");

                if (pendingStatusId == 0)
                    throw new InvalidOperationException("Pending status is not configured.");

                if (string.IsNullOrWhiteSpace(vm.ClarificationPoint))
                {
                    throw new InvalidOperationException("Clarification point is required.");
                }
                //Get Current Approval
                var currentApproval = await _context.CijWorkflowApprovals.FirstOrDefaultAsync(x =>
                            x.ApprovalId == vm.workflowApprovalId &&
                            x.Cijid == vm.cijId &&
                            x.StatusId == pendingStatusId);

                if (currentApproval == null)
                    throw new InvalidOperationException("Pending approval not found.");
                int? workflowId = await _context.CijWorkflowTransactions.Where(a => a.TransactionId == currentApproval.TransactionId)
                    .Select(x => x.WorkflowId).FirstOrDefaultAsync();


                CijWorkflowApproval? targetRole = new();
                if (workflowId == 1 || workflowId == 3)
                {
                    targetRole = await _context.CijWorkflowApprovals.Where(x => x.Cijid == vm.cijId
                                           && x.StatusId == approvedStatusId && x.ApproverRole == "HOD")
                                          .OrderByDescending(x => x.ApprovalId).FirstOrDefaultAsync();

                }
                if (workflowId == 2)
                {
                    targetRole = await _context.CijWorkflowApprovals.Where(x => x.Cijid == vm.cijId
                                           && x.StatusId == approvedStatusId && x.ApproverRole == "Director SC")
                                          .OrderByDescending(x => x.ApprovalId).FirstOrDefaultAsync();
                }

                if (targetRole == null)
                    throw new InvalidOperationException("Target approval not found.");

                bool hasRequiredRole = vm.userRoles.Any(x => x.Equals(currentApproval.ApproverRole, StringComparison.OrdinalIgnoreCase));
                if (!hasRequiredRole)
                    throw new UnauthorizedAccessException("You are not authorized.");

                // Validate department
                //if (currentApproval.DepartmentId.HasValue)
                //{
                //    if (!vm.userDepartmentId.HasValue || currentApproval.DepartmentId.Value !=vm.userDepartmentId.Value)
                //    {
                //        throw new UnauthorizedAccessException(
                //            "You are not authorized for this department.");
                //    }
                //}
                var clarification = new CijWorkflowClarification
                {
                    Cijid = vm.cijId,
                    TransactionId = currentApproval.TransactionId,
                    ApprovalId = currentApproval.ApprovalId,
                    StepId = currentApproval.StepId ?? 0,
                    RaisedBy = vm.userId,
                    RaisedByRole = currentApproval.ApproverRoleId,
                    TargetRoleId = targetRole.ApproverRoleId,
                    TargetRoleName = targetRole.ApproverRole,
                    ClarificationPoint = vm.ClarificationPoint,
                    RaisedOn = DateTime.Now,
                    StatusId = queryStatusId,
                    CreatedBy = vm.userId,
                    CreatedOn = DateTime.UtcNow
                };

                _context.CijWorkflowClarifications.Add(clarification);

                currentApproval.StatusId = queryStatusId;
                currentApproval.ActionDate = DateTime.Now;
                currentApproval.ModifiedBy = vm.userId;
                currentApproval.ModifiedOn = DateTime.Now;
                currentApproval.Remarks = vm.ClarificationPoint;

                var history = new CijWorkflowApprovalHistory
                {
                    TransactionId = currentApproval.TransactionId,
                    Cijid = vm.cijId,
                    FromStatusId = pendingStatusId,
                    ToStatusId = queryStatusId,
                    ApprovalId = currentApproval.ApprovalId,
                    ApproverUserId = vm.userId,
                    ApproverRole = currentApproval.ApproverRole,
                    ApproverRoleId = currentApproval.ApproverRoleId,
                    Remarks = vm.ClarificationPoint,
                    ActionOn = DateTime.UtcNow,
                    StepId = currentApproval.StepId,
                    CreatedBy = vm.userId,
                    CreatedOn = DateTime.Now,
                    DepartmentId = currentApproval.DepartmentId
                };

                _context.CijWorkflowApprovalHistories.Add(history);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        private async Task<bool> AnswerQueryAsync(ApproveRejectViewModel vm)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!vm.ClarificationId.HasValue)
                    throw new InvalidOperationException("Clarification ID is required.");

                if (string.IsNullOrWhiteSpace(vm.Answer))
                    throw new InvalidOperationException("Answer is required.");

                int queryStatusId = await _context.CijStatuses.Where(x => x.IsActive && x.StatusName == "Query").Select(x => x.StatusId).FirstOrDefaultAsync();
                int pendingStatusId = await _context.CijStatuses.Where(x => x.IsActive && x.StatusName == "Pending").Select(x => x.StatusId).FirstOrDefaultAsync();
                int approvedStatusId = await _context.CijStatuses.Where(x => x.IsActive && x.StatusName == "Approved").Select(x => x.StatusId).FirstOrDefaultAsync();
                int answeredStatusId = await _context.CijStatuses.Where(x => x.IsActive && x.StatusName == "Answered").Select(x => x.StatusId).FirstOrDefaultAsync();

                var clarification = await _context.CijWorkflowClarifications.FirstOrDefaultAsync(x =>
                            x.ClarificationId == vm.ClarificationId.Value && x.Cijid == vm.cijId && x.StatusId == queryStatusId);

                if (clarification == null)
                    throw new InvalidOperationException("Query not found or already answered.");

                bool isHod = vm.userRoles.Any(x => x.Equals("HOD", StringComparison.OrdinalIgnoreCase));

                if (!isHod)
                    throw new UnauthorizedAccessException("Only HOD can answer this clarification.");
                var approverRole = await _context.Roles.FirstOrDefaultAsync(x => x.Name == "HOD");
                if (approverRole == null)
                    throw new UnauthorizedAccessException("HOD Role is not configured");
                // ========================================================
                // Update clarification
                // ========================================================
                clarification.Answer = vm.Answer;
                clarification.AnsweredBy = vm.userId;
                clarification.AnsweredByRole = approverRole.Id;
                clarification.AnsweredOn = DateTime.Now;
                clarification.StatusId = answeredStatusId;
                clarification.ModifiedBy = vm.userId;
                clarification.ModifiedOn = DateTime.Now;

                var approval = await _context.CijWorkflowApprovals.FirstOrDefaultAsync(x =>
                            x.ApprovalId == clarification.ApprovalId && x.Cijid == vm.cijId);

                if (approval == null)
                    throw new InvalidOperationException("Workflow approval not found.");

                approval.StatusId = pendingStatusId;
                approval.ModifiedBy = vm.userId;
                approval.ModifiedOn = DateTime.UtcNow;
                approval.ActionDate = DateTime.Now;

                var history = new CijWorkflowApprovalHistory
                {
                    TransactionId = clarification.TransactionId,
                    Cijid = vm.cijId,
                    FromStatusId = queryStatusId,
                    ToStatusId = pendingStatusId,
                    ApprovalId = clarification.ApprovalId,
                    ApproverUserId = vm.userId,
                    ApproverRole = approverRole.Name,
                    ApproverRoleId = approverRole.Id,
                    Remarks = vm.Answer,
                    ActionOn = DateTime.Now,
                    StepId = clarification.StepId,
                    CreatedBy = vm.userId,
                    CreatedOn = DateTime.Now,
                    DepartmentId = approval.DepartmentId
                };

                _context.CijWorkflowApprovalHistories.Add(history);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        private async Task<bool> RejectWorkflowAsync(ApproveRejectViewModel vm)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int rejectedStatusId = await _context.CijStatuses.Where(x => x.IsActive && x.StatusName == "Rejected").Select(x => x.StatusId).FirstOrDefaultAsync();

                var approval = await _context.CijWorkflowApprovals.FirstOrDefaultAsync(x =>
                          x.ApprovalId == vm.workflowApprovalId && x.Cijid == vm.cijId);

                if (approval == null)
                    throw new InvalidOperationException("Workflow approval not found.");

                // 1. Update current approval
                approval.StatusId = rejectedStatusId;
                approval.Remarks = vm.remarks;
                approval.ActionDate = DateTime.Now;
                approval.ModifiedOn = DateTime.Now;
                approval.ModifiedBy = vm.userId;

                // 2. Close the workflow transaction
                var trans = await _context.CijWorkflowTransactions.Where(x =>
                          x.TransactionId == approval.TransactionId && x.Cijid == vm.cijId && x.IsActive == true).FirstOrDefaultAsync();
                if (trans == null)
                    throw new InvalidOperationException("Workflow Transction not found.");

                trans.CurrentStatusId = rejectedStatusId;
                trans.IsActive = false;
                trans.CompletionDate = DateTime.Now;
                trans.Remarks = vm.remarks;
                trans.ModifiedBy = vm.userId;
                trans.ModifiedOn = DateTime.Now;

                // 3. Close the CIJ request
                var request = await _context.CijRequests.Where(x => x.Cijid == vm.cijId && x.IsActive == true).FirstOrDefaultAsync();
                if (request == null)
                    throw new InvalidOperationException("Workflow CIJ_Request not found.");
                request.StatusId = rejectedStatusId;

                var history = new CijWorkflowApprovalHistory
                {
                    TransactionId = trans.TransactionId,
                    Cijid = request.Cijid,
                    FromStatusId = approval.StatusId,
                    ToStatusId = rejectedStatusId,
                    ApprovalId = approval.ApprovalId,
                    ApproverUserId = vm.userId,
                    ApproverRole = approval.ApproverRole,
                    ApproverRoleId = approval.ApproverRoleId,
                    Remarks = vm.Answer,
                    ActionOn = DateTime.Now,
                    StepId = approval.StepId,
                    CreatedBy = vm.userId,
                    CreatedOn = DateTime.Now,
                    DepartmentId = approval.DepartmentId
                };

                _context.CijWorkflowApprovalHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        private async Task<CijWorkflowStep?> GetWorkflowNextStep(CijWorkflowStep currentStep, CijRequest cijRequest, int WorkflowId)
        {
            CijWorkflowStep? nextStep = new CijWorkflowStep();
            List<string> FHDepartments = new List<string>() { "IT", "BME", "Admin", "Finance", "Purchase", "Civil", "Legal" };

            if (currentStep.StepCode.Equals("REQUESTOR", StringComparison.OrdinalIgnoreCase))
            {
                var requestorDept = await _context.CijDepartments.Where(x => x.DepartmentId == cijRequest.RequestDepartmentId).Select(x => x.DepartmentName).FirstOrDefaultAsync();
                var fhApprovalLimit = await (from budget in _context.CijRoleBudgetLimits
                                             join role in _context.Roles
                                           on budget.RoleId equals role.Id
                                             where role.Name == "Function Head"
                                             select budget.BudgetLimit).FirstOrDefaultAsync();
                if (FHDepartments.Contains(requestorDept ?? "", StringComparer.OrdinalIgnoreCase) && cijRequest.TotalEquipmentCost <= fhApprovalLimit)
                {
                    string stepCode = "Function_Head_Initial";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Function Head Initial Approval step is not configured.");
                    }
                }
                else
                {
                    string stepCode = "HOD_Initial";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("HOD initial Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("Function_Head_Initial", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Technical_Assessment";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Technical Assessment Approval step is not configured.");
                }
            }
            else if (currentStep.StepCode.Equals("HOD_Initial", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Technical_Assessment";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Technical Assessment Approval step is not configured.");
                }
            }
            else if (currentStep.StepCode.Equals("Technical_Assessment", StringComparison.OrdinalIgnoreCase))
            {
                string? itemtype = await _context.CijItemTypes.Where(x => x.ItemTypeId == cijRequest.ItemTypeId).Select(x => x.ItemTypeCode).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(itemtype))
                {
                    throw new InvalidOperationException("Item Type is not configured.");
                }
                if (itemtype.Equals("Medical", StringComparison.OrdinalIgnoreCase))
                {
                    string stepCode = "Purchase_Committee";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Purchase Committee Approval step is not configured.");
                    }
                }
                else
                {
                    var requestorDept = await _context.CijDepartments.Where(x => x.DepartmentId == cijRequest.RequestDepartmentId).Select(x => x.DepartmentName).FirstOrDefaultAsync();
                    var fhApprovalLimit = await (from budget in _context.CijRoleBudgetLimits
                                                 join role in _context.Roles
                                               on budget.RoleId equals role.Id
                                                 where role.Name == "Function Head"
                                                 select budget.BudgetLimit).FirstOrDefaultAsync();
                    if (FHDepartments.Contains(requestorDept ?? "", StringComparer.OrdinalIgnoreCase) && cijRequest.TotalEquipmentCost <= fhApprovalLimit)
                    {
                        string stepCode = "Function_Head_Final";
                        nextStep = await GetStep(stepCode, WorkflowId);
                        if (nextStep == null)
                        {
                            throw new InvalidOperationException("Function Head Final Approval step is not configured.");
                        }
                    }
                    else
                    {
                        string stepCode = "HOD_Final";
                        nextStep = await GetStep(stepCode, WorkflowId);
                        if (nextStep == null)
                        {
                            throw new InvalidOperationException("HOD Final Approval step is not configured.");
                        }
                    }
                }
            }
            else if (currentStep.StepCode.Equals("Purchase_Committee", StringComparison.OrdinalIgnoreCase))
            {
                var requestorDept = await _context.CijDepartments.Where(x => x.DepartmentId == cijRequest.RequestDepartmentId).Select(x => x.DepartmentName).FirstOrDefaultAsync();
                var fhApprovalLimit = await (from budget in _context.CijRoleBudgetLimits
                                             join role in _context.Roles
                                           on budget.RoleId equals role.Id
                                             where role.Name == "Function Head"
                                             select budget.BudgetLimit).FirstOrDefaultAsync();
                if (FHDepartments.Contains(requestorDept ?? "", StringComparer.OrdinalIgnoreCase) && cijRequest.TotalEquipmentCost <= fhApprovalLimit)
                {
                    string stepCode = "Function_Head_Final";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Function Head Final Approval step is not configured.");
                    }
                }
                else
                {
                    string stepCode = "HOD_Final";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("HOD Final Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("Function_Head_Final", StringComparison.OrdinalIgnoreCase))
            {
                if (cijRequest.BudgetAvailable.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    string stepCode = "FINANCE";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Finance Approval step is not configured.");
                    }
                }
                else
                {
                    string stepCode = "COO_PRE_FINANCE";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("COO Approval Pre Finance Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("HOD_Final", StringComparison.OrdinalIgnoreCase))
            {
                if (cijRequest.BudgetAvailable.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    string stepCode = "FINANCE";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Finance Approval step is not configured.");
                    }
                }
                else
                {
                    string stepCode = "COO_PRE_FINANCE";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("COO Approval Pre Finance Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("COO_PRE_FINANCE", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "FINANCE";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Finance Approval step is not configured.");
                }
            }
            else if (currentStep.StepCode.Equals("FINANCE", StringComparison.OrdinalIgnoreCase))
            {
                var hodApprovalLimit = await (from budget in _context.CijRoleBudgetLimits
                                              join role in _context.Roles
                                            on budget.RoleId equals role.Id
                                              where role.Name == "HOD" && role.IsActive == true && budget.IsActive == true
                                              select budget).FirstOrDefaultAsync();
                if (hodApprovalLimit == null)
                {
                    throw new InvalidOperationException("Budget Limit is not configured for HOD Role.");
                }
                if (hodApprovalLimit.BudgetLimit >= cijRequest.TotalEquipmentCost)
                {
                    string stepCode = "Purchase";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Purchase step is not configured.");
                    }
                }
                else
                {
                    var cijItemType = await _context.CijItemTypes.Where(x => x.ItemTypeId == cijRequest.ItemTypeId && x.IsActive == true).FirstOrDefaultAsync();
                    if (cijItemType == null)
                    {
                        throw new InvalidOperationException("Item Type is not configured.");
                    }
                    if (cijItemType.ItemTypeCode.Equals("Medical", StringComparison.OrdinalIgnoreCase))
                    {
                        string stepCode = "MD";
                        nextStep = await GetStep(stepCode, WorkflowId);
                        if (nextStep == null)
                        {
                            throw new InvalidOperationException("MD Approval step is not configured.");
                        }
                    }
                    if (cijItemType.ItemTypeCode.Equals("Non Medical", StringComparison.OrdinalIgnoreCase))
                    {
                        string stepCode = "COO_POST_FINANCE";
                        nextStep = await GetStep(stepCode, WorkflowId);
                        if (nextStep == null)
                        {
                            throw new InvalidOperationException("COO Post Finance Approval step is not configured.");
                        }
                    }
                }
            }
            else if (currentStep.StepCode.Equals("MD", StringComparison.OrdinalIgnoreCase))
            {
                var mdApprovalLimit = await (from budget in _context.CijRoleBudgetLimits
                                             join role in _context.Roles
                                           on budget.RoleId equals role.Id
                                             where role.Name == "MD" && role.IsActive == true && budget.IsActive == true
                                             select budget).FirstOrDefaultAsync();
                if (mdApprovalLimit == null)
                {
                    throw new InvalidOperationException("Budget Limit is not configured for MD Role.");
                }
                if (mdApprovalLimit.BudgetLimit >= cijRequest.TotalEquipmentCost)
                {
                    string stepCode = "Purchase";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Purchase step is not configured.");
                    }
                }
                else
                {
                    string stepCode = "CEO";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("CEO Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("COO_POST_FINANCE", StringComparison.OrdinalIgnoreCase))
            {
                var cooApprovalLimit = await (from budget in _context.CijRoleBudgetLimits
                                              join role in _context.Roles
                                            on budget.RoleId equals role.Id
                                              where role.Name == "COO" && role.IsActive == true && budget.IsActive == true
                                              select budget).FirstOrDefaultAsync();
                if (cooApprovalLimit == null)
                {
                    throw new InvalidOperationException("Budget Limit is not configured for COO Role.");
                }
                if (cooApprovalLimit.BudgetLimit >= cijRequest.TotalEquipmentCost)
                {
                    string stepCode = "Purchase";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Purchase step is not configured.");
                    }
                }
                else
                {
                    string stepCode = "CEO";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("CEO Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("CEO", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Purchase";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Purchase step is not configured.");
                }
            }
            else if (currentStep.StepCode.Equals("Purchase", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Completed";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Completed step is not configured.");
                }
            }
            else
            {
                throw new InvalidOperationException($"Workflow step '{currentStep.StepName}' is not configured.");
            }

            return nextStep;
        }

        private async Task<CijWorkflowStep?> GetStep(string StepCode, int? workflowId)
        {
            CijWorkflowStep? nextStepFind = await _context.CijWorkflowSteps.Where(x => x.WorkflowId == workflowId
               && x.IsActive && x.StepCode == StepCode).OrderBy(x => x.StepNo).FirstOrDefaultAsync();

            return nextStepFind;
        }

        public async Task<List<RequestTrackingViewModel>> TrackRequsterRequestAsync(string userId)
        {
            var pendingStatusId = await _context.CijStatuses.Where(x => x.StatusName == "Pending").Select(x => x.StatusId).FirstOrDefaultAsync();

            var results = await _context
                .Set<RequestTrackingViewModel>()
                .FromSqlInterpolated($@"
        EXEC dbo.SP_GetRequesterRequestTracking
            @UserId = {userId},
            @PendingStatusId = {pendingStatusId}")
                .AsNoTracking()
                .ToListAsync();

            return results;
        }
        private async Task<CijWorkflowStep?> GetSecondWorkflowNextStep(CijWorkflowStep currentStep, CijRequest cijRequest, int WorkflowId)
        {
            CijWorkflowStep? nextStep = new CijWorkflowStep();
            if (currentStep.StepCode.Equals("REQUESTOR", StringComparison.OrdinalIgnoreCase))
            {
                string? itemtype = await _context.CijItemTypes.Where(x => x.ItemTypeId == cijRequest.ItemTypeId).Select(x => x.ItemTypeCode).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(itemtype))
                {
                    throw new InvalidOperationException("Item Type is not configured.");
                }
                if (itemtype.Equals("Medical", StringComparison.OrdinalIgnoreCase))
                {
                    string stepCode = "Consultant_Incharge";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Consultant Incharge Approval step is not configured.");
                    }
                }
                else
                {
                    string stepCode = "Administrator";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Administrator Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("Consultant_Incharge", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Director_SC_Initial";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Director SC Initial Approval step is not configured.");
                }
            }
            else if (currentStep.StepCode.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Director_SC_Initial";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Director SC Initial Approval step is not configured.");
                }
            }
            else if (currentStep.StepCode.Equals("Director_SC_Initial", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Technical_Assessment";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Technical Assessment Approval step is not configured.");
                }
            }
            else if (currentStep.StepCode.Equals("Technical_Assessment", StringComparison.OrdinalIgnoreCase))
            {
                string? itemtype = await _context.CijItemTypes.Where(x => x.ItemTypeId == cijRequest.ItemTypeId).Select(x => x.ItemTypeCode).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(itemtype))
                {
                    throw new InvalidOperationException("Item Type is not configured.");
                }
                if (itemtype.Equals("Medical", StringComparison.OrdinalIgnoreCase))
                {
                    string stepCode = "Purchase_Committee";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Purchase Committee Approval step is not configured.");
                    }
                }
                else
                {
                    string stepCode = "Director_SC_Final";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Director SC Final Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("Purchase_Committee", StringComparison.OrdinalIgnoreCase))
            {

                string stepCode = "Director_SC_Final";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Function Head Final Approval step is not configured.");
                }
            }
            else if (currentStep.StepCode.Equals("Director_SC_Final", StringComparison.OrdinalIgnoreCase))
            {
                if (cijRequest.BudgetAvailable.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    string stepCode = "FINANCE";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Finance Approval step is not configured.");
                    }
                }
                else
                {
                    string stepCode = "COO_PRE_FINANCE";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("COO Approval Pre Finance Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("COO_PRE_FINANCE", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "FINANCE";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Finance Approval step is not configured.");
                }
            }
            else if (currentStep.StepCode.Equals("FINANCE", StringComparison.OrdinalIgnoreCase))
            {
                var directorApprovalLimit = await (from budget in _context.CijRoleBudgetLimits
                                                   join role in _context.Roles
                                                 on budget.RoleId equals role.Id
                                                   where role.Name == "Director SC" && role.IsActive == true && budget.IsActive == true
                                                   select budget).FirstOrDefaultAsync();
                if (directorApprovalLimit == null)
                {
                    throw new InvalidOperationException("Budget Limit is not configured for Director SC Role.");
                }
                if (directorApprovalLimit.BudgetLimit >= cijRequest.TotalEquipmentCost)
                {
                    string stepCode = "Purchase";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Purchase step is not configured.");
                    }
                }
                else
                {
                    var cijItemType = await _context.CijItemTypes.Where(x => x.ItemTypeId == cijRequest.ItemTypeId && x.IsActive == true).FirstOrDefaultAsync();
                    if (cijItemType == null)
                    {
                        throw new InvalidOperationException("Item Type is not configured.");
                    }
                    if (cijItemType.ItemTypeCode.Equals("Medical", StringComparison.OrdinalIgnoreCase))
                    {
                        string stepCode = "MD";
                        nextStep = await GetStep(stepCode, WorkflowId);
                        if (nextStep == null)
                        {
                            throw new InvalidOperationException("MD Approval step is not configured.");
                        }
                    }
                    if (cijItemType.ItemTypeCode.Equals("Non Medical", StringComparison.OrdinalIgnoreCase))
                    {
                        string stepCode = "COO_POST_FINANCE";
                        nextStep = await GetStep(stepCode, WorkflowId);
                        if (nextStep == null)
                        {
                            throw new InvalidOperationException("COO Post Finance Approval step is not configured.");
                        }
                    }
                }
            }
            else if (currentStep.StepCode.Equals("MD", StringComparison.OrdinalIgnoreCase))
            {
                var mdApprovalLimit = await (from budget in _context.CijRoleBudgetLimits
                                             join role in _context.Roles
                                           on budget.RoleId equals role.Id
                                             where role.Name == "MD" && role.IsActive == true && budget.IsActive == true
                                             select budget).FirstOrDefaultAsync();
                if (mdApprovalLimit == null)
                {
                    throw new InvalidOperationException("Budget Limit is not configured for MD Role.");
                }
                if (mdApprovalLimit.BudgetLimit >= cijRequest.TotalEquipmentCost)
                {
                    string stepCode = "Purchase";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Purchase step is not configured.");
                    }
                }
                else
                {
                    string stepCode = "CEO";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("CEO Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("COO_POST_FINANCE", StringComparison.OrdinalIgnoreCase))
            {
                var cooApprovalLimit = await (from budget in _context.CijRoleBudgetLimits
                                              join role in _context.Roles
                                            on budget.RoleId equals role.Id
                                              where role.Name == "COO" && role.IsActive == true && budget.IsActive == true
                                              select budget).FirstOrDefaultAsync();
                if (cooApprovalLimit == null)
                {
                    throw new InvalidOperationException("Budget Limit is not configured for COO Role.");
                }
                if (cooApprovalLimit.BudgetLimit >= cijRequest.TotalEquipmentCost)
                {
                    string stepCode = "Purchase";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Purchase step is not configured.");
                    }
                }
                else
                {
                    string stepCode = "CEO";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("CEO Approval step is not configured.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("CEO", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Purchase";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Purchase step is not configured.");
                }
            }
            else if (currentStep.StepCode.Equals("Purchase", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Completed";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Completed step is not configured.");
                }
            }
            else
            {
                throw new InvalidOperationException($"Workflow step '{currentStep.StepName}' is not configured.");
            }

            return nextStep;
        }
        private async Task<CijWorkflowStep?> GetThirdWorkflowNextStep(CijWorkflowStep currentStep, CijRequest cijRequest, int WorkflowId)
        {
            CijWorkflowStep? nextStep = new CijWorkflowStep();
            if (currentStep.StepCode.Equals("REQUESTOR", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "HOD_Initial";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("HOD initial Approval step is not configured for WF003");
                }
            }
            else if (currentStep.StepCode.Equals("HOD_Initial", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Technical_Assessment";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Technical Assessment Approval step is not configured for WF003");
                }
            }
            else if (currentStep.StepCode.Equals("Technical_Assessment", StringComparison.OrdinalIgnoreCase))
            {
                string? itemtype = await _context.CijItemTypes.Where(x => x.ItemTypeId == cijRequest.ItemTypeId).Select(x => x.ItemTypeCode).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(itemtype))
                {
                    throw new InvalidOperationException("Item Type is not configured.");
                }
                if (itemtype.Equals("Medical", StringComparison.OrdinalIgnoreCase))
                {
                    string stepCode = "Purchase_Committee";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Purchase Committee Approval step is not configured for WF003.");
                    }
                }
                else
                {
                    string stepCode = "HOD_Final";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("HOD Final Approval step is not configured for WF003.");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("Purchase_Committee", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "HOD_Final";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("HOD Final Approval step is not configured for WF003.");
                }
            }
            else if (currentStep.StepCode.Equals("HOD_Final", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "FINANCE";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Finance Approval step is not configured for WF003.");
                }
            }
            else if (currentStep.StepCode.Equals("FINANCE", StringComparison.OrdinalIgnoreCase))
            {
                var hodApprovalLimit = await (from budget in _context.CijRoleBudgetLimits
                                              join role in _context.Roles
                                            on budget.RoleId equals role.Id
                                              where role.Name == "Director Public Health" && role.IsActive == true && budget.IsActive == true
                                              select budget).FirstOrDefaultAsync();
                if (hodApprovalLimit == null)
                {
                    throw new InvalidOperationException("Budget Limit is not configured for Director Public Health for WF003.");
                }
                if (hodApprovalLimit.BudgetLimit >= cijRequest.TotalEquipmentCost)
                {
                    string stepCode = "Purchase";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Purchase step is not configured for WF003");
                    }
                }
                else
                {
                    string stepCode = "Director_Public_Health ";
                    nextStep = await GetStep(stepCode, WorkflowId);
                    if (nextStep == null)
                    {
                        throw new InvalidOperationException("Director Public Health Approval step is not configured for WF003");
                    }
                }
            }
            else if (currentStep.StepCode.Equals("Director_Public_Health", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Purchase";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Purchase step is not configured for WF003.");
                }
            }
            else if (currentStep.StepCode.Equals("Purchase", StringComparison.OrdinalIgnoreCase))
            {
                string stepCode = "Completed";
                nextStep = await GetStep(stepCode, WorkflowId);
                if (nextStep == null)
                {
                    throw new InvalidOperationException("Completed step is not configured.");
                }
            }
            else
            {
                throw new InvalidOperationException($"Workflow step '{currentStep.StepName}' is not configured.");
            }

            return nextStep;
        }

        public async Task<ApprovalRequestDetailsViewModel?> GetRequestDetailsAsync(int approvalId, int cijId)
        {
            var requestvm = await (from req in _context.CijRequests
                                   join dept in _context.CijDepartments
                                   on req.RequestDepartmentId equals dept.DepartmentId
                                   select new CIJRequestViewModel()
                                   {
                                       Cijid = req.Cijid,
                                       CIJSNumber = req.Cijnumber,
                                       RequestDate = req.RequestDate,
                                       TotalEquipmentCost = req.TotalEquipmentCost,
                                       
                                   }).FirstOrDefaultAsync();

            ApprovalRequestDetailsViewModel vm = new ApprovalRequestDetailsViewModel()
            {
                CIJRequest = requestvm
            };
            return vm;
        }
    }
}
