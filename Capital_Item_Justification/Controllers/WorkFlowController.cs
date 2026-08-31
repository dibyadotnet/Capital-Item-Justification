using Azure.Core;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace Capital_Item_Justification.Controllers
{
    public class WorkFlowController : Controller
    {
        private readonly ILogger<WorkFlowController> _logger;
        private readonly IWorkflowService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICIJRequestService _cijService;
        private readonly IEmailService _emailService;
        public WorkFlowController(ILogger<WorkFlowController> logger, IWorkflowService service, UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager, ICIJRequestService cijService, IEmailService emailService)
        {
            _logger = logger;
            _service = service;
            _userManager = userManager;
            _roleManager = roleManager;
            _cijService = cijService;
            _emailService = emailService;
        }
        public async Task<IActionResult> MyApproval()
        {
            List<MyApprovalViewModel> vm = new();
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var roles = await _userManager.GetRolesAsync(user);
                List<string> roleIds = new();

                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);

                    if (role != null)
                    {
                        roleIds.Add(role.Id);
                    }
                }
                vm = await _service.GetMyApprovalAsync(user, roleIds);
            }
            catch (Exception)
            {

                throw;
            }

            return View(vm);
        }
        public async Task<IActionResult> ApprovalDetail(int approvalId, int cijId)
        {
            ApprovalRequestDetailsViewModel? vm = new();
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var roles = await _userManager.GetRolesAsync(user);

                vm = await _service.GetRequestDetailsAsync(approvalId, cijId);

            }
            catch (Exception)
            {
                throw;
            }
            return View(vm);
        }

        public async Task<IActionResult> ApprovalAction(int approvalId, int cijId)
        {
            ApprovalDetailViewModel? vm = new();
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var roles = await _userManager.GetRolesAsync(user);

                var departmentsList = await _cijService.GetDepartment();
                var departmentItems = departmentsList.Select(x => new SelectListItem
                {
                    Value = x.DepartmentId.ToString(),
                    Text = x.DepartmentName
                }).ToList();
                vm = await _service.GetApprovalDetailAsync(approvalId, cijId);

                var isHod = roles.Any(x => x.Equals("HOD", StringComparison.OrdinalIgnoreCase));
                var hasPendingClarification = vm?.Clarifications.Any(x => x.StatusName == "Query"
                                                && x.TargetRoleName != null
                                                && x.TargetRoleName.Equals("HOD", StringComparison.OrdinalIgnoreCase));

                if (isHod == true && hasPendingClarification == true)
                {
                    vm.CanAnswerClarification = true;
                }
                vm.CanRaiseClarification = true;

                List<string> workflowStepItem = new List<string>() { "HOD_Initial", "Function_Head_Initial" };
                if (vm != null)
                {
                    vm.Departments = departmentItems;
                    vm.workflowStepList = workflowStepItem;
                }

            }
            catch (Exception)
            {

                throw;
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WorkFlowApproval(int workflowApprovalId, int cijId, List<int> assignedDept, string remarks, string action,string cijNumber)
        {
            try
            {
                ApproveRejectViewModel vm = new();
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var roles = await _userManager.GetRolesAsync(user);
                vm = new ApproveRejectViewModel()
                {
                    userId = user.Id,
                    userRoles = roles.ToList(),
                    cijId = cijId,
                    assignedDept = assignedDept,
                    remarks = remarks,
                    workflowApprovalId = workflowApprovalId,
                    userDepartmentId = user.DepartmentId,
                    Action = action,
                };
                bool? approved = await _service.ApproveRequestAsync(vm);
                await _emailService.SendEmailAsync(cijNumber, action, remarks);
                if (approved == true)
                {
                    TempData["ToastMessage"] = "CIJ request is approved successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "CIJ request failed to approve.";
                    TempData["ToastType"] = "error";
                }
                return RedirectToAction("MyApproval");
            }

            catch (Exception)
            {
                TempData["ToastMessage"] = "CIJ request failed to approve.";
                TempData["ToastType"] = "error";
                throw;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendClarification(int workflowApprovalId, int cijId, string clarificationPoint)
        {
            try
            {
                ApproveRejectViewModel vm = new();
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var roles = await _userManager.GetRolesAsync(user);
                string action = "Query";
                vm = new ApproveRejectViewModel()
                {
                    userId = user.Id,
                    userRoles = roles.ToList(),
                    cijId = cijId,
                    workflowApprovalId = workflowApprovalId,
                    userDepartmentId = user.DepartmentId,
                    Action = action,
                    ClarificationPoint = clarificationPoint
                };
                bool? approved = await _service.ApproveRequestAsync(vm);
                if (approved == true)
                {
                    TempData["ToastMessage"] = "Clarification Query submitted successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Clarification Query failed to Submit.";
                    TempData["ToastType"] = "error";
                }
                return RedirectToAction("MyApproval");
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "Clarification Query failed to Submit.";
                TempData["ToastType"] = "error";
                throw;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnswerClarification(int cijId, int clarificationId, string answer)
        {
            try
            {
                ApproveRejectViewModel vm = new();
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var roles = await _userManager.GetRolesAsync(user);
                string action = "Answer";
                vm = new ApproveRejectViewModel()
                {
                    userId = user.Id,
                    userRoles = roles.ToList(),
                    cijId = cijId,
                    ClarificationId = clarificationId,
                    userDepartmentId = user.DepartmentId,
                    Action = action,
                    Answer = answer
                };
                bool? approved = await _service.ApproveRequestAsync(vm);
                if (approved == true)
                {
                    TempData["ToastMessage"] = "Clarification answer submitted successfully..";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Clarification answer failed to Submit.";
                    TempData["ToastType"] = "error";
                }
                return RedirectToAction("MyApproval");
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "Clarification answer failed to Submit.";
                TempData["ToastType"] = "error";
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectWorkFlow(int workflowApprovalId, int cijId, string remarks)
        {
            try
            {
                ApproveRejectViewModel vm = new();
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var roles = await _userManager.GetRolesAsync(user);
                string action = "Rejected";
                vm = new ApproveRejectViewModel()
                {
                    userId = user.Id,
                    userRoles = roles.ToList(),
                    cijId = cijId,
                    remarks = remarks,
                    workflowApprovalId = workflowApprovalId,
                    userDepartmentId = user.DepartmentId,
                    Action = action,
                };
                bool? approved = await _service.ApproveRequestAsync(vm);
                if (approved == true)
                {
                    TempData["ToastMessage"] = "CIJ request is Rejected successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "CIJ request failed to reject.";
                    TempData["ToastType"] = "error";
                }
                return RedirectToAction("MyApproval");
            }

            catch (Exception)
            {
                TempData["ToastMessage"] = "CIJ request failed to reject.";
                TempData["ToastType"] = "error";
                throw;
            }
        }

        public async Task<IActionResult> TrackRequest()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var trackRequsetResults = await _service.TrackRequsterRequestAsync(user.Id);

                return View(trackRequsetResults);

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
