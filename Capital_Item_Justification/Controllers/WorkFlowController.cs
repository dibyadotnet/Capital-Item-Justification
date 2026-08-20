using Azure.Core;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

namespace Capital_Item_Justification.Controllers
{
    public class WorkFlowController : Controller
    {
        private readonly ILogger<WorkFlowController> _logger;
        private readonly IWorkflowService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICIJRequestService _cijService;
        public WorkFlowController(ILogger<WorkFlowController> logger, IWorkflowService service, UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager, ICIJRequestService cijService)
        {
            _logger = logger;
            _service = service;
            _userManager = userManager;
            _roleManager = roleManager;
            _cijService = cijService;
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
        [HttpPost]
        public async Task<IActionResult> SubmitRequest(CIJMainViewModel model)
        {
            try
            {
                //_httpContextAccessor.HttpContext.User
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var roles = await _userManager.GetRolesAsync(user);
                model.userId = user.Id;
                model.userRoles = roles.ToList();
                await _service.SubmitCIJAsync(model);

                TempData["ToastMessage"] = "CIJ request submitted successfully.";
                TempData["ToastType"] = "success";

                return RedirectToAction("Dashboard", "CIJ");
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "Unable to submit the CIJ request.";
                TempData["ToastType"] = "error";

                return RedirectToAction("Dashboard", "CIJ");
            }
        }

        public async Task<IActionResult> ApprovalDetail(int id)
        {
            ApprovalDetailViewModel? vm = new();
            try
            {
                var departmentsList = await _cijService.GetDepartment();
                var departmentItems = departmentsList.Select(x => new SelectListItem
                {
                    Value = x.DepartmentId.ToString(),
                    Text = x.DepartmentName
                }).ToList();
                vm = await _service.GetApprovalDetailAsync(id);
                vm.Departments = departmentItems;

            }
            catch (Exception)
            {

                throw;
            }
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WorkFlowApproval(int workflowApprovalId, int cijId, List<int> assignedDept, string remarks)
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
                    userDepartmentId = user.DepartmentId
                };
               bool? approved= await _service.ApproveRequestAsync(vm);
                if (approved==true)
                {
                    TempData["ToastMessage"] = "CIJ request is approved.";
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
    }
}
