using Azure.Core;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Services;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Pdf.Filters;
using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        private readonly CIJDbContext _cIJDbContext;
        private string? workingRoleId => HttpContext.Session.GetString("WorkingRoleId");
        private string? workingRoleName => HttpContext.Session.GetString("WorkingRoleName");
        private string? assignmentId => HttpContext.Session.GetString("WorkingAssignmentId");
        private int? workingDepartmentId => HttpContext.Session.GetInt32("WorkingDepartmentId");
        private int? workingLocationId => HttpContext.Session.GetInt32("WorkingLocationId");
        public WorkFlowController(ILogger<WorkFlowController> logger, IWorkflowService service, UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager, ICIJRequestService cijService, IEmailService emailService, CIJDbContext cIJDbContext)
        {
            _logger = logger;
            _service = service;
            _userManager = userManager;
            _roleManager = roleManager;
            _cijService = cijService;
            _emailService = emailService;
            _cIJDbContext = cIJDbContext;
        }
        public async Task<IActionResult> MyApproval(int? departmentId, int? locationId, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            List<MyApprovalViewModel> vm = new();
            MyApprovalListViewModel model = new MyApprovalListViewModel();
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                List<string> roleIds = new();

                //var roles = await _userManager.GetRolesAsync(user);
                //foreach (var roleName in roles)
                //{
                //    var role = await _roleManager.FindByNameAsync(roleName);

                //    if (role != null)
                //    {
                //        roleIds.Add(role.Id);
                //    }
                //}
                if (string.IsNullOrEmpty(workingRoleId) || string.IsNullOrEmpty(assignmentId))
                {
                    return RedirectToAction("SelectRole", "Account");
                }
                roleIds = new() { workingRoleId };
                user.DepartmentId = workingDepartmentId;
                user.LocationId = workingLocationId;

                vm = await _service.GetMyApprovalAsync(user, roleIds);
                vm = vm.Where(a => a.StatusName != "Completed").ToList();
                ViewBag.Page = "Pending";

                var departmentsList = await _cijService.GetDepartment();
                var locationList = await _cijService.GetLocation();
                ViewBag.Locations = new SelectList(locationList, "LocationId", "LocationName", locationId);
                ViewBag.Departments = new SelectList(departmentsList, "DepartmentId", "DepartmentName", departmentId);

                if (departmentId.HasValue)
                {
                    vm = vm.Where(x => x.DepartmentId == departmentId.Value).ToList();
                }
                if (locationId.HasValue)
                {
                    vm = vm.Where(x => x.LocationId == locationId.Value).ToList();
                }

                if (fromDate.HasValue)
                {
                    vm = vm.Where(x =>
                        x.RequestDate.HasValue &&
                        x.RequestDate.Value >= fromDate.Value.Date).ToList();
                }

                if (toDate.HasValue)
                {
                    var endDateExclusive = toDate.Value.Date.AddDays(1);

                    vm = vm.Where(x =>
                        x.RequestDate.HasValue &&
                        x.RequestDate.Value < endDateExclusive).ToList();
                }

                var totalRecords = vm.Count();
                var otherRequests = vm.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                model = new MyApprovalListViewModel
                {
                    myApprovalViewModels = otherRequests,
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = totalPages,
                        ControllerName = "WorkFlow",
                        ActionName = "MyApproval"
                    }
                };
            }
            catch (Exception)
            {

                throw;
            }

            return View(model);
        }
        public async Task<IActionResult> CompletedApproval(int? departmentId, int? locationId, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            List<CompletedRequestViewModel> vm = new();
            CompletedRequestsListViewModel model = new();
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                List<string> roleIds = new();

                if (string.IsNullOrEmpty(workingRoleId) || string.IsNullOrEmpty(assignmentId))
                {
                    return RedirectToAction("SelectRole", "Account");
                }
                roleIds = new() { workingRoleId };
                user.DepartmentId = workingDepartmentId;
                user.LocationId = workingLocationId;
                //vm = await _service.GetMyApprovalAsync(user, roleIds);

                vm = await _cIJDbContext.Database
                  .SqlQueryRaw<CompletedRequestViewModel>(
                      "EXEC dbo.SP_GetCompletedRequests").ToListAsync();

                vm = vm.Where(a => a.StatusName == "Completed").ToList();
                ViewBag.Page = "Completed";

                var departmentsList = await _cijService.GetDepartment();
                var locationList = await _cijService.GetLocation();
                ViewBag.Locations = new SelectList(locationList, "LocationId", "LocationName", locationId);
                ViewBag.Departments = new SelectList(departmentsList, "DepartmentId", "DepartmentName", departmentId);


                if (departmentId.HasValue)
                {
                    vm = vm.Where(x => x.DepartmentId == departmentId.Value).ToList();
                }
                if (locationId.HasValue)
                {
                    vm = vm.Where(x => x.LocationId == locationId.Value).ToList();
                }

                if (fromDate.HasValue)
                {
                    vm = vm.Where(x =>
                        x.RequestDate.HasValue &&
                        x.RequestDate.Value >= fromDate.Value.Date).ToList();
                }

                if (toDate.HasValue)
                {
                    var endDateExclusive = toDate.Value.Date.AddDays(1);

                    vm = vm.Where(x =>
                        x.RequestDate.HasValue &&
                        x.RequestDate.Value < endDateExclusive).ToList();
                }

                var totalRecords = vm.Count();
                var otherRequests = vm.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                model = new CompletedRequestsListViewModel
                {
                    completedRequestViewModels = otherRequests,
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = totalPages,
                        ControllerName = "WorkFlow",
                        ActionName = "CompletedApproval"
                    }
                };
            }
            catch (Exception ex)
            {
                throw;
            }
            return View(model);
        }
        public async Task<IActionResult> ViewCIJRequest(int approvalId, int cijId)
        {
            ApprovalRequestDetailsViewModel? vm = new();
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                //var roles = await _userManager.GetRolesAsync(user);

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
                //var roles = await _userManager.GetRolesAsync(user);
                var departmentsList = await _cijService.GetDepartment();
                var allowedDepartments = new[] { "BME", "IT", "Admin" };
                var departmentItems = departmentsList.Where(x => allowedDepartments.Contains(
                                            x.DepartmentName.Trim(), StringComparer.OrdinalIgnoreCase))
                                            .Select(x => new SelectListItem
                                            {
                                                Value = x.DepartmentId.ToString(),
                                                Text = x.DepartmentName
                                            }).ToList();
                vm = await _service.GetApprovalDetailAsync(approvalId, cijId);
                if (vm == null)
                    throw new InvalidOperationException();

                //var isHod = roles.Any(x => x.Equals("HOD", StringComparison.OrdinalIgnoreCase));
                //HOD or Function head
                var hasPendingClarification = vm?.Clarifications.Any(x => x.StatusName == "Query"
                                                && x.TargetRoleName != null
                                                && workingRoleName != null
                                                && workingRoleName.Contains(x.TargetRoleName));

                if (hasPendingClarification == true)
                {
                    vm.CanAnswerClarification = true;
                }
                vm.CanRaiseClarification = true;

                //Fetch Target Role for send query
                var rolevm = await _service.GetRoleToSendQueryAsync(approvalId, cijId);
                vm.TargetRoles = rolevm.Select(x => new SelectListItem
                {
                    Value = x.ApprovalId.ToString(),
                    Text = x.RoleName
                }).ToList();

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
        public async Task<IActionResult> WorkFlowApproval(int workflowApprovalId, int cijId, List<int> assignedDept, string remarks, string action, string cijNumber, List<IFormFile>? PurchaseAttachments)
        {
            try
            {
                ApproveRejectViewModel vm = new();
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                //var roles = await _userManager.GetRolesAsync(user);
                user.DepartmentId = workingDepartmentId;
                user.LocationId = workingLocationId;
                vm = new ApproveRejectViewModel()
                {
                    userId = user.Id,
                    userRoles = string.IsNullOrEmpty(workingRoleName) ? new List<string>() : new List<string> { workingRoleName },
                    cijId = cijId,
                    assignedDept = assignedDept,
                    remarks = remarks,
                    workflowApprovalId = workflowApprovalId,
                    userDepartmentId = user.DepartmentId,
                    Action = action,
                    PurchaseAttachments = PurchaseAttachments
                };
                bool? approved = await _service.ApproveRequestAsync(vm);
                if (approved == true)
                {
                    await _emailService.SendEmailAsync(cijId, action, null, null);
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
        public async Task<IActionResult> SendClarification(int workflowApprovalId, int cijId, string clarificationPoint, int approvalId)
        {
            try
            {
                ApproveRejectViewModel vm = new();
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                //var roles = await _userManager.GetRolesAsync(user);
                user.DepartmentId = workingDepartmentId;
                user.LocationId = workingLocationId;
                string action = "Query";
                vm = new ApproveRejectViewModel()
                {
                    userId = user.Id,
                    userRoles = string.IsNullOrEmpty(workingRoleName) ? new List<string>() : new List<string> { workingRoleName }, //roles.ToList(),
                    cijId = cijId,
                    workflowApprovalId = workflowApprovalId,
                    userDepartmentId = user.DepartmentId,
                    Action = action,
                    ClarificationPoint = clarificationPoint,
                    approvalId = approvalId
                };
                bool? approved = await _service.ApproveRequestAsync(vm);
                if (approved == true)
                {
                    await _emailService.SendEmailAsync(cijId, action, approvalId, null);
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
                //var roles = await _userManager.GetRolesAsync(user);
                user.DepartmentId = workingDepartmentId;
                user.LocationId = workingLocationId;
                string action = "Answer";
                vm = new ApproveRejectViewModel()
                {
                    userId = user.Id,
                    userRoles = string.IsNullOrEmpty(workingRoleName) ? new List<string>() : new List<string> { workingRoleName }, //roles.ToList(),
                    cijId = cijId,
                    ClarificationId = clarificationId,
                    userDepartmentId = user.DepartmentId,
                    Action = action,
                    Answer = answer
                };
                bool? approved = await _service.ApproveRequestAsync(vm);
                if (approved == true)
                {
                    await _emailService.SendEmailAsync(cijId, action, null, clarificationId);
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
                //var roles = await _userManager.GetRolesAsync(user);
                user.DepartmentId = workingDepartmentId;
                user.LocationId = workingLocationId;
                string action = "Rejected";
                vm = new ApproveRejectViewModel()
                {
                    userId = user.Id,
                    userRoles = string.IsNullOrEmpty(workingRoleName) ? new List<string>() : new List<string> { workingRoleName }, //roles.ToList(),
                    cijId = cijId,
                    remarks = remarks,
                    workflowApprovalId = workflowApprovalId,
                    userDepartmentId = user.DepartmentId,
                    Action = action,
                };
                bool? approved = await _service.ApproveRequestAsync(vm);
                if (approved == true)
                {
                    await _emailService.SendEmailAsync(cijId, action, null, null);
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

        public async Task<IActionResult> TrackRequest(int? cijId, int? statusId, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var cijList = await _cijService.GetCijNumberListAsync(user.Id);
                var statusList = await _cijService.GetStatusListAsync();
                statusList = statusList.Where(x => x.StatusName == "Pending" || x.StatusName == "Completed" || x.StatusName == "Rejected").ToList();
                ViewBag.CIJNumberList = new SelectList(cijList, "CijId", "CIJNumber", cijId);
                ViewBag.StatusList = new SelectList(statusList, "StatusId", "StatusName", statusId);

                var trackRequsetResults = await _service.TrackRequsterRequestAsync(user.Id, cijId, statusId);

                if (fromDate.HasValue)
                {
                    trackRequsetResults = trackRequsetResults.Where(x =>
                        x.RequestDate.HasValue &&
                        x.RequestDate.Value >= fromDate.Value.Date).ToList();
                }

                if (toDate.HasValue)
                {
                    var endDateExclusive = toDate.Value.Date.AddDays(1);

                    trackRequsetResults = trackRequsetResults.Where(x =>
                        x.RequestDate.HasValue &&
                        x.RequestDate.Value < endDateExclusive).ToList();
                }

                var totalRecords = trackRequsetResults.Count();
                var trackMyRequests = trackRequsetResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                var model = new TrackMyRequestListViewModel
                {
                    requestTrackings = trackMyRequests,
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = totalPages,
                        ControllerName = "WorkFlow",
                        ActionName = "TrackRequest"
                    }
                };


                return View(model);

            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        public async Task<IActionResult> OtherRoleRequests(int? departmentId, int? locationId, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
        {
            List<MyApprovalViewModel> vm = new();
            MyApprovalListViewModel model = new MyApprovalListViewModel();
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                //var roles = await _userManager.GetRolesAsync(user);
                var departmentsList = await _cijService.GetDepartment();
                var locationList = await _cijService.GetLocation();
                ViewBag.Locations = new SelectList(locationList, "LocationId", "LocationName", locationId);
                ViewBag.Departments = new SelectList(departmentsList, "DepartmentId", "DepartmentName", departmentId);

                vm = await _service.GetOtherRoleRequestsAsync();
                List<string> roles = string.IsNullOrEmpty(workingRoleName) ? new List<string>() : new List<string> { workingRoleName };

                if (roles.Any(r => r.Contains("Purchase", StringComparison.OrdinalIgnoreCase)))
                {
                    vm = vm.Where(x => !(x.ApproverRole?.Contains("Purchase", StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
                }
                ViewBag.Page = "Pending/Query/Rejected";
                if (departmentId.HasValue)
                {
                    vm = vm.Where(x => x.DepartmentId == departmentId.Value).ToList();
                }
                if (locationId.HasValue)
                {
                    vm = vm.Where(x => x.LocationId == locationId.Value).ToList();
                }

                if (fromDate.HasValue)
                {
                    vm = vm.Where(x =>
                        x.RequestDate.HasValue &&
                        x.RequestDate.Value >= fromDate.Value.Date).ToList();
                }

                if (toDate.HasValue)
                {
                    var endDateExclusive = toDate.Value.Date.AddDays(1);

                    vm = vm.Where(x =>
                        x.RequestDate.HasValue &&
                        x.RequestDate.Value < endDateExclusive).ToList();
                }

                var totalRecords = vm.Count();
                var otherRequests = vm.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                model = new MyApprovalListViewModel
                {
                    myApprovalViewModels = otherRequests,
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = totalPages,
                        ControllerName = "WorkFlow",
                        ActionName = "OtherRoleRequests"
                    }
                };
            }
            catch (Exception ex)
            {
                throw;
            }

            return View(model);
        }
    }
}
