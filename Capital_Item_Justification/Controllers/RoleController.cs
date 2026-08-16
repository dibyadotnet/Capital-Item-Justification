using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Capital_Item_Justification.Models;
using System.Data;

namespace Capital_Item_Justification.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RoleController(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
            return View(roles);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View(new RoleViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);

            if (roleExists)
            {
                ModelState.AddModelError( "RoleName","Role already exists.");

                return View("CreateRole",model);
            }

            var role = new ApplicationRole
            {
                Name = model.RoleName,
                CreatedBy = "Admin",
                CreatedOn = DateTime.Now
            };

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                TempData["ToastMessage"] ="Role created successfully.";
                TempData["ToastType"] = "success";

                return RedirectToAction(nameof(GetRoles));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty,error.Description);
            }

            return View("CreateRole",model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var role = await _roleManager.FindByIdAsync(id);

                if (role == null)
                    return NotFound();

                role.IsActive = !role.IsActive;
                role.ModifiedBy = User.Identity?.Name;
                role.ModifiedOn = DateTime.UtcNow;

                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                {
                    TempData["ToastMessage"] ="Unable to change role status.";
                    TempData["ToastType"] = "Error";
                    return RedirectToAction(nameof(GetRoles));
                }
                if (role.IsActive)
                {
                    TempData["ToastMessage"] = "Role activated successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Role deactivated successfully.";
                    TempData["ToastType"] = "Error";
                }

                return RedirectToAction(nameof(GetRoles));
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,"Error while changing status for role {RoleId}", id);
                TempData["ToastMessage"] ="Unable to change role status.";
                TempData["ToastType"] = "Error";
                return RedirectToAction(nameof(GetRoles));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                if (string.IsNullOrEmpty(model.Id))
                    return BadRequest();

                var role = await _roleManager.FindByIdAsync(model.Id);

                if (role == null || !role.IsActive)
                    return NotFound();

                var existingRole = await _roleManager.FindByNameAsync(model.RoleName);

                if (existingRole != null && existingRole.Id != role.Id)
                {
                    ModelState.AddModelError("RoleName","Role name already exists.");
                    return View(model);
                }

                role.Name = model.RoleName.Trim();
                role.NormalizedName = model.RoleName.Trim().ToUpperInvariant();
                role.ModifiedBy = User.Identity?.Name;
                role.ModifiedOn = DateTime.Now;

                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }

                TempData["ToastMessage"] = "Role updated successfully.";
                TempData["ToastType"] = "success";

                return RedirectToAction(nameof(GetRoles));
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,"Error while updating role {RoleId}", model.Id);

                TempData["ToastMessage"] ="Unable to update role. Please try again.";
                TempData["ToastType"] = "Error";
                return RedirectToAction(nameof(GetRoles));
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null || !role.IsActive)
                return NotFound();

            var model = new RoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name!
            };

            return View("EditRole",model);
        }
    }
}


