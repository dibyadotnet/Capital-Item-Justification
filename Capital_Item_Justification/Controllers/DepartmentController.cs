using Capital_Item_Justification.Models;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Capital_Item_Justification.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDeptService _service;
        private readonly ILogger<DepartmentController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public DepartmentController(IDeptService service, ILogger<DepartmentController> logger, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _logger = logger;
            _userManager = userManager;
        }
        public async Task<IActionResult> GetAllDept()
        {
            List<DepartmentViewModel> depts = new List<DepartmentViewModel>();
            try
            {
                depts = await _service.GetAllAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return View(depts);
        }
        [HttpGet]
        public IActionResult AddDepartment()
        {
            try
            {
                DepartmentViewModel vm = new DepartmentViewModel();
                return View("AddDepartment",vm);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditDepartment(int id)
        {
            try
            {
                DepartmentViewModel? vm = await _service.GetByIdAsync(id);
                if (vm == null)
                    throw new InvalidOperationException();

                return View("AddDepartment", vm);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public async Task<IActionResult> Save(DepartmentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("AddDepartment", model);
                }
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var exists = await _service.DeptExistsAsync(model.DepartmentName, model.DepartmentId);

                if (exists)
                {
                    ModelState.AddModelError(nameof(model.DepartmentName), "Department already exists.");
                    return View("AddDepartment", model);
                }
                bool saved = await _service.SaveAsync(model, user.Id);
                if (saved)
                {
                    TempData["ToastMessage"] = "Department Saved successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Department failed to save.";
                    TempData["ToastType"] = "error";
                }
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "Department failed to save.";
                TempData["ToastType"] = "error";
                throw;
            }

            return RedirectToAction("GetAllDept");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteDepartment(int id)
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
                    TempData["ToastMessage"] = "Department Deleted successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Department failed to Delete.";
                    TempData["ToastType"] = "error";
                }
                return RedirectToAction("GetAllDept");
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "Department failed to Delete";
                TempData["ToastType"] = "error";
                throw;
            }
        }
    }
}
