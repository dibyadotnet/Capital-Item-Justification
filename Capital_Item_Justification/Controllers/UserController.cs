
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public UserController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.OrderBy(x => x.FullName).ToListAsync();

            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new UserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                return View("CreateUser", model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,

                EmployeeCode = model.EmployeeCode,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,

                DepartmentId = model.DepartmentId,
                LocationId = model.LocationId,

                IsActive = model.IsActive,

                CreatedOn = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name
            };

            var result = await _userManager.CreateAsync(user, model.Password ?? "Welcome@123");

            if (result.Succeeded)
            {
                //TempData["SuccessMessage"] = "User created successfully.";
                TempData["ToastMessage"] = "User created successfully.";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(GetUsers));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("GetUsers");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                Id = user.Id,
                EmployeeCode = user.EmployeeCode ?? "",
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,

                DepartmentId = user.DepartmentId,
                LocationId = user.LocationId,

                IsActive = user.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Id))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            user.EmployeeCode = model.EmployeeCode;
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;

            user.PhoneNumber = model.PhoneNumber;

            user.DepartmentId = model.DepartmentId;
            user.LocationId = model.LocationId;

            user.IsActive = model.IsActive;

            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = User.Identity?.Name;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                //TempData["SuccessMessage"] = "User updated successfully.";
                TempData["ToastMessage"] = "User updated successfully.";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(GetUsers));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RedirectToAction("GetUsers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;

            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = User.Identity?.Name;

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(GetUsers));
        }
        [HttpGet]
        public async Task<IActionResult> ManageRoles(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var assignedRoles =
                await _userManager.GetRolesAsync(user);

            var availableRoles =
                await _userManager.GetRolesAsync(user);

            var allRoles = await _userManager.GetRolesAsync(user);

            var roleNames = await GetAllRoleNames();

            var model = new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.FullName ?? user.Email ?? "",

                AssignedRoles = assignedRoles.ToList(),

                AvailableRoles = roleNames
            };

            return View(model);
        }

        private async Task<List<string>> GetAllRoleNames()
        {
            return await _roleManager.Roles
                .Where(x => x.Name != null && x.IsActive)
                .Select(x => x.Name!)
                .OrderBy(x => x)
                .ToListAsync();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(string userId, List<string> selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var currentRoles =
                await _userManager.GetRolesAsync(user);

            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update existing roles.");

                return RedirectToAction(
                    nameof(ManageRoles),
                    new { id = userId });
            }

            if (selectedRoles != null &&
                selectedRoles.Any())
            {
                var addResult =
                    await _userManager.AddToRolesAsync(
                        user,
                        selectedRoles);

                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError( string.Empty,error.Description);
                    }

                    return RedirectToAction(nameof(ManageRoles), new { id = userId });
                }
            }

            //TempData["SuccessMessage"] = "User roles updated successfully.";
            TempData["ToastMessage"] = "User roles updated successfully.";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(GetUsers));
        }
    }
}
