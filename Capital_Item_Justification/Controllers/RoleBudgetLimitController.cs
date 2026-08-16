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
        public RoleBudgetLimitController(CIJDbContext context, RoleManager<ApplicationRole> roleManager,
            ICIJRequestService service)
        {
            _context = context;
            _roleManager = roleManager;
            _service = service;
        }
        // GET: /RoleBudgetLimit
        public async Task<IActionResult> GeRoleBudgetLimit()
        {
            //var data = await _context.CijRoleBudgetLimits
            //    .Include(x => x.Role)
            //    .OrderBy(x => x.Role!.Name)
            //    .Select(x => new RoleBudgetLimitViewModel
            //    {
            //        RoleBudgetLimitId = x.RoleBudgetLimitId,
            //        RoleId = x.RoleId,
            //        RoleName = x.Role!.Name!,
            //        BudgetLimit = x.BudgetLimit,
            //        IsActive = x.IsActive
            //    })
            //    .ToListAsync();
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
        ItemTypeName = itemType != null ? itemType.ItemTypeName : "",
        BudgetLimit = budget.BudgetLimit,
        IsActive = budget.IsActive
    }
).ToListAsync();

            return View(data);
        }

        // GET: /RoleBudgetLimit/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            ViewBag.Roles = roles;

            //Item Type Master
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

        // POST: /RoleBudgetLimit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleBudgetLimitViewModel model)
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
                ModelState.AddModelError("RoleId", "Invalid role selected.");

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

            await _context.SaveChangesAsync();

            //TempData["SuccessMessage"] ="Role budget limit added successfully.";
            TempData["ToastMessage"] = "Role budget limit added successfully.";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(GeRoleBudgetLimit));
        }

        // GET: /RoleBudgetLimit/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.CijRoleBudgetLimits
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.RoleBudgetLimitId == id);

            if (entity == null)
                return NotFound();
            //Item Type Master
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
                ItemTypes = itemTypesItem,
            };

            await LoadRoles();

            return View("EditBudgetLimit", model);
        }

        // POST: /RoleBudgetLimit/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleBudgetLimitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRoles();
                return View(model);
            }

            var entity = await _context.CijRoleBudgetLimits
                .FirstOrDefaultAsync(x =>
                    x.RoleBudgetLimitId == model.RoleBudgetLimitId);

            if (entity == null)
                return NotFound();

            // Check whether another record already uses this role
            bool duplicate = await _context.CijRoleBudgetLimits
                .AnyAsync(x =>
                    x.RoleId == model.RoleId &&
                    x.RoleBudgetLimitId != model.RoleBudgetLimitId);

            if (duplicate)
            {
                ModelState.AddModelError(
                    "RoleId",
                    "Budget limit is already configured for this role.");

                await LoadRoles();
                return View("CreateBudgetLimit", model);
            }

            entity.RoleId = model.RoleId;
            entity.BudgetLimit = model.BudgetLimit;
            entity.IsActive = model.IsActive;
            entity.ModifiedBy = User.Identity?.Name;
            entity.ModifiedOn = DateTime.Now;
            entity.ItemTypeId = model.ItemTypeId;

            await _context.SaveChangesAsync();

            //TempData["SuccessMessage"] ="Role budget limit updated successfully.";
            TempData["ToastMessage"] = "Role budget limit updated successfully.";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(GeRoleBudgetLimit));
        }

        private async Task LoadRoles()
        {
            ViewBag.Roles = await _roleManager.Roles
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }

}
