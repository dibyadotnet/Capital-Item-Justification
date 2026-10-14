using Capital_Item_Justification.Models;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FunctionHeadDepartmentController : Controller
    {
        private readonly IFunctionHeadDepartmentService _service;
        private readonly ILogger<FunctionHeadDepartmentController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICIJRequestService _cijRequestService;
        public FunctionHeadDepartmentController(IFunctionHeadDepartmentService service, ILogger<FunctionHeadDepartmentController> logger, UserManager<ApplicationUser> userManager, ICIJRequestService cijRequestService)
        {
            _service = service;
            _logger = logger;
            _userManager = userManager;
            _cijRequestService = cijRequestService;
        }
        public async Task<IActionResult> GetAllFHDept(int page = 1, int pageSize = 10)
        {
            List<FunctionHeadDepartmentViewModel> fhDepts = new List<FunctionHeadDepartmentViewModel>();
            var model = new FHDepartmentListViewModel();
            try
            {
                if (page < 1)
                    page = 1;

                if (pageSize <= 0)
                    pageSize = 10;

                fhDepts = await _service.GetAllAsync();

                var totalRecords = fhDepts.Count();
                var fhDepartments = fhDepts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                model = new FHDepartmentListViewModel
                {
                    fhDepts = fhDepartments,
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = totalPages,
                        ControllerName = "FunctionHeadDepartment",
                        ActionName = "GetAllFHDept"
                    }
                };

            }
            catch (Exception ex)
            {
                throw;
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> AddFHDepartment()
        {
            try
            {
                FunctionHeadDepartmentViewModel vm = new FunctionHeadDepartmentViewModel();
                var departmentsList = await _cijRequestService.GetDepartment();
                var departmentItems = departmentsList.Select(x => new SelectListItem
                {
                    Value = x.DepartmentId.ToString(),
                    Text = x.DepartmentName
                }).ToList();
                vm.Departments= departmentItems;
                return View("AddFHDepartment", vm);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditFHDepartment(int id)
        {
            try
            {
                FunctionHeadDepartmentViewModel? vm = await _service.GetByIdAsync(id);
                if (vm == null)
                    throw new InvalidOperationException();

                return View("AddFHDepartment", vm);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        public async Task<IActionResult> Save(FunctionHeadDepartmentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var departmentsList = await _cijRequestService.GetDepartment();
                    var departmentItems = departmentsList.Select(x => new SelectListItem
                    {
                        Value = x.DepartmentId.ToString(),
                        Text = x.DepartmentName
                    }).ToList();
                    model.Departments = departmentItems;
                    return View("AddFHDepartment", model);
                }
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var exists = await _service.DeptExistsAsync(model.DepartmentId);

                if (exists)
                {
                    ModelState.AddModelError(nameof(model.DepartmentName), "Department already exists.");
                    return View("AddFHDepartment", model);
                }
                bool saved = await _service.SaveAsync(model, user.Id);
                if (saved)
                {
                    TempData["ToastMessage"] = "Saved successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "failed to save.";
                    TempData["ToastType"] = "error";
                }
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "failed to save.";
                TempData["ToastType"] = "error";
                throw;
            }

            return RedirectToAction("GetAllFHDept");
        }
        [HttpPost]
        public async Task<IActionResult> ManageStaus(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                bool deleted = await _service.DeleteAsync(id, user.Id);
                if (deleted)
                {
                    TempData["ToastMessage"] = "Status updated successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Status failed to Update.";
                    TempData["ToastType"] = "error";
                }
                return RedirectToAction("GetAllFHDept");
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "Status failed to Update.";
                TempData["ToastType"] = "error";
                throw;
            }
        }
    }
}
