using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;

namespace Capital_Item_Justification.Controllers
{
    public class RoleBudgetLimitController : Controller
    {
        private readonly CIJDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICIJRequestService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<WorkFlowController> _logger;
        public RoleBudgetLimitController(CIJDbContext context, RoleManager<ApplicationRole> roleManager,
            ICIJRequestService service, UserManager<ApplicationUser> userManager, ILogger<WorkFlowController> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _service = service;
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<IActionResult> GeRoleBudgetLimit()
        {
            try
            {
                var data = await (
                    from budget in _context.CijRoleBudgetLimits

                    join role in _context.Roles
                        on budget.RoleId equals role.Id into roleGroup
                    from role in roleGroup.DefaultIfEmpty()

                    join itemType in _context.CijItemTypes
                        on budget.ItemTypeId equals itemType.ItemTypeId into itemTypeGroup
                    from itemType in itemTypeGroup.DefaultIfEmpty()

                    orderby role.Name

                    select new RoleBudgetLimitViewModel
                    {
                        RoleBudgetLimitId = budget.RoleBudgetLimitId,
                        RoleId = budget.RoleId,
                        RoleName = role != null ? role.Name : "",
                        ItemTypeId = budget.ItemTypeId,
                        ItemTypeName = itemType != null
                            ? itemType.ItemTypeName
                            : "",
                        BudgetLimit = budget.BudgetLimit,
                        IsActive = budget.IsActive
                    }).ToListAsync();

                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while retrieving role budget limits.");

                TempData["ToastMessage"] =
                    "An error occurred while retrieving role budget limits.";
                TempData["ToastType"] = "error";

                return View(new List<RoleBudgetLimitViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Name)
                    .ToListAsync();

                ViewBag.Roles = roles;

                // Item Type Master
                var itemTypes = await _service.GetItemType();

                var itemTypesItem = itemTypes.Select(x => new SelectListItem
                {
                    Value = x.ItemTypeId.ToString(),
                    Text = x.ItemTypeCode
                }).ToList();

                return View("CreateBudgetLimit", new RoleBudgetLimitViewModel
                {
                    ItemTypes = itemTypesItem,
                    IsActive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while loading the Create Budget Limit page.");

                TempData["ToastMessage"] =
                    "An error occurred while loading the page. Please try again.";
                TempData["ToastType"] = "error";

                return RedirectToAction(nameof(GeRoleBudgetLimit));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleBudgetLimitViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadRoles();
                    return View("CreateBudgetLimit", model);
                }

                // Prevent duplicate role budget
                bool exists = await _context.CijRoleBudgetLimits
                    .AnyAsync(x => x.RoleId == model.RoleId);

                if (exists)
                {
                    ModelState.AddModelError(
                        "RoleId",
                        "Budget limit is already configured for this role.");

                    await LoadRoles();
                    return View("CreateBudgetLimit", model);
                }

                var role = await _roleManager.FindByIdAsync(model.RoleId);

                if (role == null)
                {
                    ModelState.AddModelError(
                        "RoleId",
                        "Invalid role selected.");

                    await LoadRoles();
                    return View("CreateBudgetLimit", model);
                }

                var entity = new CijRoleBudgetLimit
                {
                    RoleId = model.RoleId,
                    BudgetLimit = model.BudgetLimit,
                    IsActive = true,
                    CreatedBy = User.Identity?.Name,
                    CreatedOn = DateTime.Now,
                    ItemTypeId = model.ItemTypeId
                };

                _context.CijRoleBudgetLimits.Add(entity);

                int count = await _context.SaveChangesAsync();

                if (count > 0)
                {
                    TempData["ToastMessage"] =
                        "Role budget limit added successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] =
                        "Role budget limit failed to add.";
                    TempData["ToastType"] = "error";
                }

                return RedirectToAction(nameof(GeRoleBudgetLimit));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred while creating a role budget limit for RoleId: {RoleId}",
                    model.RoleId);

                TempData["ToastMessage"] =
                    "An unexpected error occurred while adding the role budget limit.";
                TempData["ToastType"] = "error";

                await LoadRoles();

                return View("CreateBudgetLimit", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var entity = await _context.CijRoleBudgetLimits
                    .Include(x => x.Role)
                    .FirstOrDefaultAsync(x => x.RoleBudgetLimitId == id);

                if (entity == null)
                {
                    return NotFound();
                }

                // Item Type Master
                var itemTypes = await _service.GetItemType();

                var itemTypesItem = itemTypes.Select(x => new SelectListItem
                {
                    Value = x.ItemTypeId.ToString(),
                    Text = x.ItemTypeCode
                }).ToList();

                var model = new RoleBudgetLimitViewModel
                {
                    RoleBudgetLimitId = entity.RoleBudgetLimitId,
                    RoleId = entity.RoleId,
                    RoleName = entity.Role?.Name ?? "",
                    BudgetLimit = entity.BudgetLimit,
                    IsActive = entity.IsActive,
                    ItemTypeId = entity.ItemTypeId,
                    ItemTypes = itemTypesItem
                };

                await LoadRoles();

                return View("EditBudgetLimit", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while loading the Edit Budget Limit page for ID: {Id}",
                    id);

                TempData["ToastMessage"] =
                    "An error occurred while loading the budget limit details.";
                TempData["ToastType"] = "error";

                return RedirectToAction(nameof(GeRoleBudgetLimit));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleBudgetLimitViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadRoles();

                    // Reload Item Type dropdown
                    var itemTypes = await _service.GetItemType();

                    model.ItemTypes = itemTypes.Select(x => new SelectListItem
                    {
                        Value = x.ItemTypeId.ToString(),
                        Text = x.ItemTypeCode
                    }).ToList();

                    return View("EditBudgetLimit", model);
                }

                var entity = await _context.CijRoleBudgetLimits
                    .FirstOrDefaultAsync(x =>
                        x.RoleBudgetLimitId == model.RoleBudgetLimitId);

                if (entity == null)
                {
                    return NotFound();
                }

                // Update role budget limit details
                entity.RoleId = model.RoleId;
                entity.BudgetLimit = model.BudgetLimit;
                entity.IsActive = model.IsActive;
                entity.ModifiedBy = User.Identity?.Name;
                entity.ModifiedOn = DateTime.Now;
                entity.ItemTypeId = model.ItemTypeId;

                int count = await _context.SaveChangesAsync();

                if (count > 0)
                {
                    TempData["ToastMessage"] =
                        "Role budget limit updated successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] =
                        "Role budget limit failed to update.";
                    TempData["ToastType"] = "error";
                }

                return RedirectToAction(nameof(GeRoleBudgetLimit));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while updating RoleBudgetLimitId: {Id}",
                    model.RoleBudgetLimitId);

                TempData["ToastMessage"] =
                    "An unexpected error occurred while updating the role budget limit.";
                TempData["ToastType"] = "error";

                await LoadRoles();

                // Reload Item Type dropdown
                var itemTypes = await _service.GetItemType();

                model.ItemTypes = itemTypes.Select(x => new SelectListItem
                {
                    Value = x.ItemTypeId.ToString(),
                    Text = x.ItemTypeCode
                }).ToList();

                return View("EditBudgetLimit", model);
            }
        }

        private async Task LoadRoles()
        {
            ViewBag.Roles = await _roleManager.Roles
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Unauthorized();
                }

                var entity = await _context.CijRoleBudgetLimits
                    .FirstOrDefaultAsync(x => x.RoleBudgetLimitId == id);

                if (entity == null)
                {
                    TempData["ToastMessage"] = "Budget limit record not found.";
                    TempData["ToastType"] = "error";

                    return RedirectToAction(nameof(GeRoleBudgetLimit));
                }

                // Toggle active status
                entity.IsActive = !entity.IsActive;
                entity.ModifiedBy = User.Identity?.Name;
                entity.ModifiedOn = DateTime.Now;

                int count = await _context.SaveChangesAsync();

                if (count > 0)
                {
                    TempData["ToastMessage"] = "Status updated successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Status failed to update.";
                    TempData["ToastType"] = "error";
                }

                return RedirectToAction(nameof(GeRoleBudgetLimit));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while changing the status for RoleBudgetLimitId: {Id}",
                    id);

                TempData["ToastMessage"] = "An error occurred while updating the status.";
                TempData["ToastType"] = "error";

                return RedirectToAction(nameof(GeRoleBudgetLimit));
            }
        }
    }

}
