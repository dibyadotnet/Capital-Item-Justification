using Capital_Item_Justification.Models;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Capital_Item_Justification.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectService _service;
        private readonly ILogger<ProjectController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProjectController(IProjectService service, ILogger<ProjectController> logger, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _logger = logger;
            _userManager = userManager;
        }
        public async Task<IActionResult> GetProjects(int page = 1, int pageSize = 10)
        {
            List<ProjectViewModel> projects = new List<ProjectViewModel>();
            var model = new ProjectListViewModel();
            try
            {
                if (page < 1)
                    page = 1;

                if (pageSize <= 0)
                    pageSize = 10;

                projects = await _service.GetAllAsync();

                var totalRecords = projects.Count();
                var projectslst = projects.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                model = new ProjectListViewModel
                {
                    Projects = projectslst,
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = totalPages,
                        ControllerName = "Project",
                        ActionName = "GetProjects"
                    }
                };

            }
            catch (Exception)
            {
                throw;
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult AddProject()
        {
            try
            {
                ProjectViewModel vm = new ProjectViewModel();
                return View("AddProject", vm);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditProject(int id)
        {
            try
            {
                ProjectViewModel? vm = await _service.GetByIdAsync(id);
                if (vm == null)
                    throw new InvalidOperationException();

                return View("AddProject", vm);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public async Task<IActionResult> Save(ProjectViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("AddProject", model);
                }
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var exists = await _service.ProjectExistsAsync(model.ProjectCode, model.ProjectId);

                if (exists)
                {
                    ModelState.AddModelError(nameof(model.ProjectCode), "Project Code already exists.");
                    return View("AddProject", model);
                }
                bool saved = await _service.SaveAsync(model, user.Id);
                if (saved)
                {
                    TempData["ToastMessage"] = "Project Saved successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Project failed to save.";
                    TempData["ToastType"] = "error";
                }
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "Project failed to save.";
                TempData["ToastType"] = "error";
                throw;
            }

            return RedirectToAction("GetProjects");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProject(int id)
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
                return RedirectToAction("GetProjects");
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
