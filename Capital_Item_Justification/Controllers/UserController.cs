
using Azure.Core;
using Capital_Item_Justification.Data;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.Arm;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Capital_Item_Justification.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICIJRequestService _service;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly CIJDbContext _context;
        public UserController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
            ICIJRequestService service, IUserService userService, IEmailService emailService, CIJDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _service = service;
            _userService = userService;
            _emailService = emailService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(UserListRequest filter)
        {
            if (filter.page < 1)
                filter.page = 1;

            if (filter.pageSize <= 0)
                filter.pageSize = 10;

            var vm = await _userService.GetUsersAsync();
            if (filter.departmentId.HasValue)
            {
                vm = vm.Where(x => x.DepartmentId == filter.departmentId.Value).ToList();
            }
            if (filter.locationId.HasValue)
            {
                vm = vm.Where(x => x.LocationId == filter.locationId.Value).ToList();
            }
            if (!string.IsNullOrEmpty(filter.roleId))
            {
                vm = vm.Where(x => x.RoleId == filter.roleId).ToList();
            }
            var totalRecords = vm.Count();
            var users = vm.Skip((filter.page - 1) * filter.pageSize).Take(filter.pageSize).ToList();
            var totalPages = (int)Math.Ceiling((double)totalRecords / filter.pageSize);

            var departmentsList = await _service.GetDepartment();
            var locationList = await _service.GetLocation();
            var roleList = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();

            ViewBag.DepartmentList = new SelectList(departmentsList, "DepartmentId", "DepartmentName", filter.departmentId);
            ViewBag.LocationList = new SelectList(locationList, "LocationId", "LocationName", filter.locationId);
            ViewBag.RoleList = new SelectList(roleList, "Id", "Name", filter.roleId);

            var model = new UserListViewModel
            {
                Users = users,

                Pagination = new PaginationViewModel
                {
                    CurrentPage = filter.page,
                    PageSize = filter.pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    ControllerName = "User",
                    ActionName = "GetUsers"
                }
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            UserViewModel userViewModel = new UserViewModel();

            var departmentsList = await _service.GetDepartment();
            var departmentItems = departmentsList.Select(x => new SelectListItem
            {
                Value = x.DepartmentId.ToString(),
                Text = x.DepartmentName
            }).ToList();


            var locationList = await _service.GetLocation();
            var locationItems = locationList.Select(x => new SelectListItem
            {
                Value = x.LocationId.ToString(),
                Text = x.LocationName
            }).ToList();

            userViewModel.Departments = departmentItems;
            userViewModel.Locations = locationItems;
            return View(userViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            var departmentsList = await _service.GetDepartment();
            var departmentItems = departmentsList.Select(x => new SelectListItem
            {
                Value = x.DepartmentId.ToString(),
                Text = x.DepartmentName
            }).ToList();
            var locationList = await _service.GetLocation();
            var locationItems = locationList.Select(x => new SelectListItem
            {
                Value = x.LocationId.ToString(),
                Text = x.LocationName
            }).ToList();

            model.Departments = departmentItems;
            model.Locations = locationItems;

            if (!ModelState.IsValid)
            {
                return View("CreateUser", model);
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
                string firstName = model.FullName?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                await _emailService.SendUserCreationEmailAsync(firstName, user.Email);
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

            var departmentsList = await _service.GetDepartment();
            var departmentItems = departmentsList.Select(x => new SelectListItem
            {
                Value = x.DepartmentId.ToString(),
                Text = x.DepartmentName
            }).ToList();


            var locationList = await _service.GetLocation();
            var locationItems = locationList.Select(x => new SelectListItem
            {
                Value = x.LocationId.ToString(),
                Text = x.LocationName
            }).ToList();

            var model = new UserViewModel
            {
                Id = user.Id,
                EmployeeCode = user.EmployeeCode ?? "",
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                DepartmentId = user.DepartmentId??0,
                LocationId = user.LocationId??0,
                IsActive = user.IsActive,
                Departments = departmentItems,
                Locations = locationItems
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            var departmentsList = await _service.GetDepartment();
            var departmentItems = departmentsList.Select(x => new SelectListItem
            {
                Value = x.DepartmentId.ToString(),
                Text = x.DepartmentName
            }).ToList();
            var locationList = await _service.GetLocation();
            var locationItems = locationList.Select(x => new SelectListItem
            {
                Value = x.LocationId.ToString(),
                Text = x.LocationName
            }).ToList();

            model.Departments = departmentItems;
            model.Locations = locationItems;

            if (!ModelState.IsValid)
            {
                return View("EditUser", model);
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
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return RedirectToAction(nameof(ManageRoles), new { id = userId });
                }
            }

            //TempData["SuccessMessage"] = "User roles updated successfully.";
            TempData["ToastMessage"] = "User roles updated successfully.";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(GetUsers));
        }


        [HttpGet]
        public async Task<IActionResult> GetAssignments(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "User ID is required." });

            var userExists = await _userManager.FindByIdAsync(userId);

            if (userExists == null)
                return NotFound(new { message = "User not found." });

            var assignments = await _context.CijUserRoleAssignments
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.AssignmentId)
                .ToListAsync();

            var roles = await _roleManager.Roles
                .AsNoTracking()
                .ToDictionaryAsync(r => r.Id, r => r.Name ?? "");

            var departments = (await _service.GetDepartment())
                .ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var locations = (await _service.GetLocation())
                .ToDictionary(l => l.LocationId, l => l.LocationName);

            var result = assignments.Select(a =>
                new UserAssignmentViewModel
                {
                    AssignmentId = a.AssignmentId,

                    RoleId = a.RoleId,
                    RoleName = roles.GetValueOrDefault(a.RoleId, "Unknown Role"),

                    DepartmentId = a.DepartmentId,
                    DepartmentName = a.DepartmentId.HasValue
                        ? departments.GetValueOrDefault(
                            a.DepartmentId.Value, "Unknown Department")
                        : a.ScopeType == "AllDepartments"
                            ? "ALL Departments"
                            : "System",

                    LocationId = a.LocationId,
                    LocationName = a.LocationId.HasValue
                        ? locations.GetValueOrDefault(
                            a.LocationId.Value, "Unknown Location")
                        : "All Locations",

                    ScopeType = a.ScopeType,
                    IsActive = a.IsActive,
                    CreatedOn = a.CreatedOn
                }).ToList();

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAssignment([FromBody] SaveUserAssignmentRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.RoleId) ||
                !request.DepartmentId.HasValue ||
                !request.LocationId.HasValue)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "User, role, department and location are required."
                });
            }
            var loggedinUser = await _userManager.GetUserAsync(User);
            if (loggedinUser == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
                return NotFound(new { success = false, message = "User not found." });

            var role = await _roleManager.FindByIdAsync(request.RoleId);

            if (role == null || !role.IsActive)
                return BadRequest(new { success = false, message = "Invalid or inactive role." });

            var departments = await _service.GetDepartment();
            var locations = await _service.GetLocation();

            if (!departments.Any(d => d.DepartmentId == request.DepartmentId.Value))
                return BadRequest(new { success = false, message = "Invalid department." });

            if (!locations.Any(l => l.LocationId == request.LocationId.Value))
                return BadRequest(new { success = false, message = "Invalid location." });

            var duplicate = await _context.CijUserRoleAssignments.AnyAsync(a =>
                a.UserId == request.UserId &&
                a.RoleId == request.RoleId &&
                a.DepartmentId == request.DepartmentId &&
                //a.LocationId == request.LocationId &&
                //a.ScopeType == "Department" &&
                a.IsActive);

            if (duplicate)
                return Conflict(new
                {
                    success = false,
                    message = "This role, department assignment already exists."
                });

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // Identity membership is unique per user and role.
                //if (!await _userManager.IsInRoleAsync(user, role.Name!))
                //{
                //    var roleResult = await _userManager.AddToRoleAsync(user, role.Name!);

                //    if (!roleResult.Succeeded)
                //    {
                //        await transaction.RollbackAsync();

                //        return BadRequest(new
                //        {
                //            success = false,
                //            message = string.Join("; ",
                //                roleResult.Errors.Select(e => e.Description))
                //        });
                //    }
                //}
                if (request.AssignmentId > 0)
                {
                    CijUserRoleAssignment? assignment = null;
                    assignment = _context.CijUserRoleAssignments.Where(a => a.IsActive && a.AssignmentId == request.AssignmentId).FirstOrDefault();
                    if (assignment == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Assignment not found for this user."
                        });
                    }
                    assignment.RoleId = request.RoleId;
                    assignment.DepartmentId = request.DepartmentId;
                    assignment.LocationId = request.LocationId;
                    assignment.ModifiedBy = loggedinUser.Id;
                    assignment.ModifiedOn = DateTime.Now;
                }
                else
                {
                    CijUserRoleAssignment assignment = new CijUserRoleAssignment
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        DepartmentId = request.DepartmentId,
                        LocationId = request.LocationId,
                        ScopeType = "Department",
                        IsActive = true,
                        CreatedBy = loggedinUser.Id,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.CijUserRoleAssignments.Add(assignment);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Assignment saved successfully.",
                    //assignmentId = assignment.AssignmentId
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    success = false,
                    message = "Unable to save the assignment. Please try again."
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAssignmentOptions(string userId)
        {
            //var roles = await _roleManager.Roles
            //    .Where(r => r.IsActive && r.Name != null)
            //    .OrderBy(r => r.Name)
            //    .Select(r => new { id = r.Id, name = r.Name }).ToListAsync();

            var roles = await (from u in _context.UserRoles
                               join r in _context.Roles on u.RoleId equals r.Id
                               where u.UserId == userId && r.IsActive
                               select new
                               {
                                   id = r.Id,
                                   name = r.Name
                               }).ToListAsync();

            var departments = (await _service.GetDepartment())
                .Select(d => new
                {
                    id = d.DepartmentId,
                    name = d.DepartmentName
                }).ToList();

            var locations = (await _service.GetLocation())
                .Select(l => new
                {
                    id = l.LocationId,
                    name = l.LocationName
                })
                .ToList();

            return Json(new
            {
                roles,
                departments,
                locations
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignmentById(int assignmentId)
        {
            var assignment = await _context.CijUserRoleAssignments
                .AsNoTracking()
                .Where(x => x.AssignmentId == assignmentId)
                .Select(x => new
                {
                    x.AssignmentId,
                    x.UserId,
                    x.RoleId,
                    x.DepartmentId,
                    x.LocationId
                })
                .FirstOrDefaultAsync();

            if (assignment == null)
            {
                return NotFound(new { message = "Assignment not found." });
            }

            return Json(new
            {
                assignmentId = assignment.AssignmentId,
                userId = assignment.UserId,
                roleId = assignment.RoleId,
                departmentId = assignment.DepartmentId,
                locationId = assignment.LocationId
            });
        }
        public class ToggleAssignmentStatusRequest
        {
            public int AssignmentId { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAssignmentStatus([FromBody] ToggleAssignmentStatusRequest request)
        {
            var assignment = await _context.CijUserRoleAssignments
                .FirstOrDefaultAsync(x => x.AssignmentId == request.AssignmentId);

            if (assignment == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Assignment not found."
                });
            }

            assignment.IsActive = !assignment.IsActive;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = assignment.IsActive,
                message = assignment.IsActive
                    ? "Assignment activated successfully."
                    : "Assignment deactivated successfully."
            });
        }
    }
}
