using Capital_Item_Justification.Models;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Capital_Item_Justification.Controllers
{
    public class LocationController : Controller
    {
        private readonly ILocationService _service;
        private readonly ILogger<LocationController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public LocationController(ILocationService service, ILogger<LocationController> logger, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _logger = logger;
            _userManager = userManager;
        }
        public async Task<IActionResult> GetAllLocation()
        {
            List<LocationViewModel> locations = new List<LocationViewModel>();
            try
            {
                locations = await _service.GetAllAsync();
            }
            catch (Exception)
            {

                throw;
            }
            return View(locations);
        }
        [HttpGet]
        public IActionResult CreateLocation()
        {
            try
            {
                LocationViewModel locationViewModel = new LocationViewModel();
                var locationTypeItem = new List<SelectListItem>()
                {
                    new SelectListItem { Value="Primary",Text="Primary"},
                    new SelectListItem { Value="Secondary",Text="Secondary"}
                };
                locationViewModel.LocationTypeList = locationTypeItem;

                return View(locationViewModel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditLocation(int id)
        {
            try
            {
                LocationViewModel? locationViewModel = await _service.GetByIdAsync(id);
                var locationTypeItem = new List<SelectListItem>()
                {
                    new SelectListItem { Value="Primary",Text="Primary"},
                    new SelectListItem { Value="Secondary",Text="Secondary"}
                };
                if (locationViewModel == null)
                    throw new InvalidOperationException();

                locationViewModel.LocationTypeList = locationTypeItem;
                return View("CreateLocation", locationViewModel);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public async Task<IActionResult> Save(LocationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("CreateLocation", model);
                }
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var exists = await _service.LocationExistsAsync(model.LocationName, model.LocationId);

                if (exists)
                {
                    ModelState.AddModelError(nameof(model.LocationName), "Location already exists.");
                    return View("CreateLocation", model);
                }
                bool saved = await _service.SaveAsync(model, user.Id);
                if (saved)
                {
                    TempData["ToastMessage"] = "Location Saved successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Location failed to save.";
                    TempData["ToastType"] = "error";
                }
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "Location failed to save.";
                TempData["ToastType"] = "error";
                throw;
            }

            return RedirectToAction("GetAllLocation");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteLocation(int id)
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
                    TempData["ToastMessage"] = "Location Deleted successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Location failed to Delete.";
                    TempData["ToastType"] = "error";
                }
                return RedirectToAction("GetAllLocation");
            }
            catch (Exception)
            {
                TempData["ToastMessage"] = "Location failed to Delete";
                TempData["ToastType"] = "error";
                throw;
            }
        }
    }
}
