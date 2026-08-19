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
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);

                var roles = await _userManager.GetRolesAsync(user);

                var roleIds = await _roleManager.Roles.Where(r => roles.Contains(r.Name!)).Select(r => r.Id).ToListAsync();
                vm = await _service.GetMyApprovalAsync(userId, roleIds);
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
                TempData["SuccessMessage"] = "CIJ request submitted successfully.";

                return RedirectToAction("Dashboard", "CIJ");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Unable to submit the CIJ request.";
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
        public async Task<IActionResult> WorkFlowApproval(int approvalId,int cijId, List<int> assignedDept, string remarks)
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
                    userId=user.Id,
                    userRoles=roles.ToList(),
                    cijId=cijId,
                    assignedDept=assignedDept,
                    remarks=remarks,
                    workflowApprovalId=approvalId,

                };
                await _service.ApproveRequestAsync(vm);
                return View();
            }

            catch (Exception)
            {

                throw;
            }
        }
    }
}
