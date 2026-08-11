
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

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users
                .OrderBy(x => x.FullName)
                .ToListAsync();

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
                return View(model);
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
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
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
                TempData["SuccessMessage"] =
                    "User updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
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
    }
}
