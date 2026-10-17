using Capital_Item_Justification.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Capital_Item_Justification.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Capital_Item_Justification.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<WorkFlowController> _logger;
        private readonly CIJDbContext _context;
        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IEmailService emailService, ILogger<WorkFlowController> logger
            , CIJDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var loggedinUser = await _userManager.GetUserAsync(User);
                if (loggedinUser == null)
                {
                    return View();
                }
                var roles = await _userManager.GetRolesAsync(loggedinUser);
                return RedirectToAction("Dashboard", "CIJ");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }

            // Check whether user is active
            if (!user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account is inactive. Please contact the administrator.");

                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                //if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                //{
                //    return Redirect(returnUrl);
                //}
                //var loggedinUser = await _userManager.GetUserAsync(User);
                //if (loggedinUser == null)
                //{
                //    return View(model);
                //}
                //var roles = await _userManager.GetRolesAsync(loggedinUser);

                return RedirectToAction(nameof(SelectRole));

                //return RedirectToAction("Dashboard", "CIJ");

            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account has been locked due to multiple failed login attempts. Please try again later.");

                return View(model);
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Login is not allowed for this account.");

                return View(model);
            }

            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login", "Account");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var user = await _userManager.FindByEmailAsync(model.Email);

                // Don't reveal whether the email exists
                if (user == null || !user.IsActive)
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var resetLink = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new
                    {
                        userId = user.Id,
                        token = encodedToken
                    },
                    Request.Scheme);

                // TODO: Send resetLink through your email service
                await _emailService.SendPasswordResetEmailAsync(user.Email!, resetLink!);

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while processing a password reset request.");

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to process your request. Please try again later.");

                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? userId, string? token)
        {
            if (string.IsNullOrEmpty(userId) ||
                string.IsNullOrEmpty(token))
            {
                return BadRequest();
            }
            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(token));
            }
            catch
            {
                return BadRequest();
            }
            return View(new ResetPasswordViewModel
            {
                UserId = userId,
                Token = decodedToken
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId!);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid password reset request.");

                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token!,
                model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (result.Succeeded)
            {
                TempData["ToastMessage"] = "Your password has been changed successfully.";
                TempData["ToastType"] = "success";

                return RedirectToAction("Dashboard", "CIJ");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> SelectRole()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // Load the user's active role assignments.
            var assignments = await _context.CijUserRoleAssignments
                .Where(x => x.UserId == user.Id && x.IsActive)
                .Join(
                    _context.Roles,
                    assignment => assignment.RoleId,
                    role => role.Id,
                    (assignment, role) => new
                    {
                        Assignment = assignment,
                        Role = role
                    })
                .ToListAsync();

            // Only display roles the user is actually permitted to use.
            var identityRoles = await _userManager.GetRolesAsync(user);

            assignments = assignments
                .Where(x => identityRoles.Contains(x.Role.Name!))
                .ToList();

            if (assignments.Count == 0)
            {
                TempData["ErrorMessage"] =
                    "No active role assignments were found for your account.";

                return RedirectToAction("AccessDenied", "Account");
            }

            // One assignment: select it automatically.
            if (assignments.Count == 1)
            {
                return await SetWorkingRole(assignments[0].Assignment.AssignmentId);
            }

            var assignmentIds = assignments
                .Select(x => x.Assignment.DepartmentId)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var locationIds = assignments
                .Select(x => x.Assignment.LocationId)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var departments = await _context.CijDepartments
                .Where(x => assignmentIds.Contains(x.DepartmentId))
                .ToDictionaryAsync(x => x.DepartmentId, x => x.DepartmentName);

            var locations = await _context.CijLocations
                .Where(x => locationIds.Contains(x.LocationId))
                .ToDictionaryAsync(x => x.LocationId, x => x.LocationName);

            var vm = new SelectRoleViewModel
            {
                Roles = assignments.Select(x =>
                {
                    var a = x.Assignment;

                    return new RoleAssignmentOptionViewModel
                    {
                        AssignmentId = a.AssignmentId,
                        RoleId = a.RoleId,
                        RoleName = x.Role.Name ?? string.Empty,
                        DepartmentId = a.DepartmentId,
                        DepartmentName = a.DepartmentId.HasValue
                            ? departments.GetValueOrDefault(
                                a.DepartmentId.Value, "Unknown Department")
                            : "All Departments",
                        LocationId = a.LocationId,
                        LocationName = a.LocationId.HasValue
                            ? locations.GetValueOrDefault(
                                a.LocationId.Value, "Unknown Location")
                            : "All Locations",
                        ScopeType = a.ScopeType ?? string.Empty
                    };
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectRole(SelectRoleViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (!ModelState.IsValid)
            {
                return await ReloadRoleSelection(user.Id, model);
            }

            // Revalidate the assignment against the logged-in user.
            var assignment = await _context.CijUserRoleAssignments
                .FirstOrDefaultAsync(x =>
                    x.AssignmentId == model.AssignmentId &&
                    x.UserId == user.Id &&
                    x.IsActive);

            if (assignment == null)
            {
                ModelState.AddModelError(
                    nameof(model.AssignmentId),
                    "The selected role assignment is invalid or inactive.");

                return await ReloadRoleSelection(user.Id, model);
            }

            // Confirm the role is also assigned in ASP.NET Core Identity.
            var identityRoles = await _userManager.GetRolesAsync(user);

            var role = await _context.Roles
                .FirstOrDefaultAsync(x => x.Id == assignment.RoleId);

            if (role == null ||
                string.IsNullOrWhiteSpace(role.Name) ||
                !identityRoles.Contains(role.Name))
            {
                return Forbid();
            }

            return await SetWorkingRole(assignment.AssignmentId);
        }
        private async Task<IActionResult> ReloadRoleSelection(string userId, SelectRoleViewModel model)
        {
            var assignments = await _context.CijUserRoleAssignments
                .Where(x => x.UserId == userId && x.IsActive)
            .Join(
                    _context.Roles,
                    a => a.RoleId,
                    r => r.Id,
                    (a, r) => new { Assignment = a, Role = r })
                .ToListAsync();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Challenge();

            var identityRoles = await _userManager.GetRolesAsync(user);

            assignments = assignments
                .Where(x => identityRoles.Contains(x.Role.Name!))
                .ToList();

            var departmentIds = assignments
                .Where(x => x.Assignment.DepartmentId.HasValue)
                .Select(x => x.Assignment.DepartmentId!.Value)
                .Distinct()
                .ToList();

            var locationIds = assignments
                .Where(x => x.Assignment.LocationId.HasValue)
                .Select(x => x.Assignment.LocationId!.Value)
                .Distinct()
            .ToList();

            var departments = await _context.CijDepartments
                .Where(x => departmentIds.Contains(x.DepartmentId))
                .ToDictionaryAsync(x => x.DepartmentId, x => x.DepartmentName);

            var locations = await _context.CijLocations
                .Where(x => locationIds.Contains(x.LocationId))
                .ToDictionaryAsync(x => x.LocationId, x => x.LocationName);

            model.Roles = assignments.Select(x => new RoleAssignmentOptionViewModel
            {
                AssignmentId = x.Assignment.AssignmentId,
                RoleId = x.Assignment.RoleId,
                RoleName = x.Role.Name ?? string.Empty,
                DepartmentId = x.Assignment.DepartmentId,
                DepartmentName = x.Assignment.DepartmentId.HasValue
                    ? departments.GetValueOrDefault(
                        x.Assignment.DepartmentId.Value, "Unknown Department")
                    : "All Departments",
                LocationId = x.Assignment.LocationId,
                LocationName = x.Assignment.LocationId.HasValue
                    ? locations.GetValueOrDefault(
                        x.Assignment.LocationId.Value, "Unknown Location")
                    : "All Locations",
                ScopeType = x.Assignment.ScopeType ?? string.Empty
            }).ToList();

            return View("SelectRole", model);
        }
        private async Task<IActionResult> SetWorkingRole(int assignmentId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var assignment = await _context.CijUserRoleAssignments
                .FirstOrDefaultAsync(x =>
                    x.AssignmentId == assignmentId &&
                    x.UserId == user.Id &&
                    x.IsActive);

            if (assignment == null)
                return Forbid();

            var role = await _context.Roles
                .FirstOrDefaultAsync(x => x.Id == assignment.RoleId);

            var identityRoles = await _userManager.GetRolesAsync(user);

            if (role?.Name == null || !identityRoles.Contains(role.Name))
                return Forbid();

            // Store only the validated, selected working context.
            HttpContext.Session.SetString("WorkingAssignmentId",
                assignment.AssignmentId.ToString());

            HttpContext.Session.SetString("WorkingRoleId",
                assignment.RoleId);

            HttpContext.Session.SetString("WorkingRoleName",
                role.Name);

            if (assignment.DepartmentId.HasValue)
            {
                HttpContext.Session.SetInt32(
                    "WorkingDepartmentId", assignment.DepartmentId.Value);
            }
            else
            {
                HttpContext.Session.Remove("WorkingDepartmentId");
            }

            if (assignment.LocationId.HasValue)
            {
                HttpContext.Session.SetInt32(
                    "WorkingLocationId", assignment.LocationId.Value);
            }
            else
            {
                HttpContext.Session.Remove("WorkingLocationId");
            }
            return RedirectToAction("Dashboard", "CIJ");
        }
    }
}
