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
                //var loggedinUser = await _userManager.GetUserAsync(User);
                //if (loggedinUser == null)
                //{
                //    return View();
                //}
                //var roles = await _userManager.GetRolesAsync(loggedinUser);
                //return RedirectToAction("Dashboard", "CIJ");

                // Clear the existing Identity authentication cookie
                await _signInManager.SignOutAsync();
                HttpContext.Session.Clear();
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
                _logger.LogInformation("Login Successfully {isSuccess}", result.Succeeded);
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
            var correlationId = HttpContext.TraceIdentifier;

            _logger.LogInformation(
                "SelectRole started. IsAuthenticated={IsAuthenticated}", User.Identity?.IsAuthenticated);

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogWarning(
                        "SelectRole: No authenticated user found. TraceId={TraceId}. Returning Challenge.",
                        correlationId);

                    return Challenge();
                }

                _logger.LogInformation(
                    "SelectRole: User found. UserId={UserId}, Email={Email}, TraceId={TraceId}",
                    user.Id,
                    user.Email,
                    correlationId);

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

                _logger.LogInformation(
                    "SelectRole: Active assignments loaded. UserId={UserId}, Count={Count}",
                    user.Id,
                    assignments.Count);

                // Only display roles the user is actually permitted to use.
                var identityRoles = await _userManager.GetRolesAsync(user);

                _logger.LogInformation(
                    "SelectRole: Identity roles loaded. UserId={UserId}, RoleCount={RoleCount}, Roles={Roles}",
                    user.Id,
                    identityRoles.Count,
                    string.Join(", ", identityRoles));

                var activeAssignmentCount = assignments.Count;

                assignments = assignments
                    .Where(x => identityRoles.Contains(x.Role.Name!))
                    .ToList();

                _logger.LogInformation(
                    "SelectRole: Assignment filtering completed. UserId={UserId}, BeforeFilter={BeforeFilter}, AfterFilter={AfterFilter}",
                    user.Id,
                    activeAssignmentCount,
                    assignments.Count);

                if (assignments.Count == 0)
                {
                    _logger.LogWarning(
                        "SelectRole: No valid active assignments found. UserId={UserId}, TraceId={TraceId}",
                        user.Id,
                        correlationId);

                    TempData["ErrorMessage"] =
                        "No active role assignments were found for your account.";

                    _logger.LogInformation(
                        "SelectRole: Redirecting to Account/AccessDenied. TraceId={TraceId}",
                        correlationId);

                    return RedirectToAction("AccessDenied", "Account");
                }

                // One assignment: select it automatically.
                if (assignments.Count == 1)
                {
                    var assignmentId = assignments[0].Assignment.AssignmentId;

                    _logger.LogInformation(
                        "SelectRole: Exactly one valid assignment. Automatically selecting AssignmentId={AssignmentId}, Role={RoleName}, TraceId={TraceId}",
                        assignmentId,
                        assignments[0].Role.Name,
                        correlationId);

                    return await SetWorkingRole(assignmentId);
                }

                _logger.LogInformation(
                    "SelectRole: Multiple valid assignments found. Building role selection view. UserId={UserId}, Count={Count}",
                    user.Id,
                    assignments.Count);

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

                _logger.LogInformation(
                    "SelectRole: Loading lookup data. DepartmentCount={DepartmentCount}, LocationCount={LocationCount}",
                    assignmentIds.Count,
                    locationIds.Count);

                var departments = await _context.CijDepartments
                    .Where(x => assignmentIds.Contains(x.DepartmentId))
                    .ToDictionaryAsync(
                        x => x.DepartmentId,
                        x => x.DepartmentName);

                var locations = await _context.CijLocations
                    .Where(x => locationIds.Contains(x.LocationId))
                    .ToDictionaryAsync(
                        x => x.LocationId,
                        x => x.LocationName);

                _logger.LogInformation(
                    "SelectRole: Lookup data loaded. Departments={DepartmentCount}, Locations={LocationCount}",
                    departments.Count,
                    locations.Count);

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
                                    a.DepartmentId.Value,
                                    "Unknown Department")
                                : "All Departments",
                            LocationId = a.LocationId,
                            LocationName = a.LocationId.HasValue
                                ? locations.GetValueOrDefault(
                                    a.LocationId.Value,
                                    "Unknown Location")
                                : "All Locations",
                            ScopeType = a.ScopeType ?? string.Empty
                        };
                    }).ToList()
                };

                _logger.LogInformation(
                    "SelectRole: Returning SelectRole view. UserId={UserId}, ViewRoleCount={ViewRoleCount}, TraceId={TraceId}",
                    user.Id,
                    vm.Roles.Count,
                    correlationId);

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SelectRole failed. TraceId={TraceId}, IsAuthenticated={IsAuthenticated}",
                    correlationId,
                    User.Identity?.IsAuthenticated);

                throw;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectRole(SelectRoleViewModel model)
        {
            var traceId = HttpContext.TraceIdentifier;

            _logger.LogInformation(
                "SelectRole POST started. TraceId={TraceId}, IsAuthenticated={IsAuthenticated}, AssignmentId={AssignmentId}",
                traceId,
                User.Identity?.IsAuthenticated,
                model.AssignmentId);

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogWarning(
                        "SelectRole POST: User not found. Returning Challenge. TraceId={TraceId}",
                        traceId);

                    return Challenge();
                }

                _logger.LogInformation(
                    "SelectRole POST: User found. UserId={UserId}, Email={Email}",
                    user.Id,
                    user.Email);

                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e =>
                            $"{x.Key}: {e.ErrorMessage}"))
                        .ToList();

                    _logger.LogWarning(
                        "SelectRole POST: Model validation failed. UserId={UserId}, Errors={Errors}",
                        user.Id,
                        string.Join(" | ", validationErrors));

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
                    _logger.LogWarning(
                        "SelectRole POST: Assignment not found, inactive, or not owned by user. UserId={UserId}, AssignmentId={AssignmentId}",
                        user.Id,
                        model.AssignmentId);

                    ModelState.AddModelError(
                        nameof(model.AssignmentId),
                        "The selected role assignment is invalid or inactive.");

                    return await ReloadRoleSelection(user.Id, model);
                }

                _logger.LogInformation(
                    "SelectRole POST: Assignment validated. AssignmentId={AssignmentId}, RoleId={RoleId}, DepartmentId={DepartmentId}, LocationId={LocationId}",
                    assignment.AssignmentId,
                    assignment.RoleId,
                    assignment.DepartmentId,
                    assignment.LocationId);

                // Confirm the role is also assigned in ASP.NET Core Identity.
                var identityRoles = await _userManager.GetRolesAsync(user);

                var role = await _context.Roles
                    .FirstOrDefaultAsync(x => x.Id == assignment.RoleId);

                if (role == null ||
                    string.IsNullOrWhiteSpace(role.Name) ||
                    !identityRoles.Contains(role.Name))
                {
                    _logger.LogWarning(
                        "SelectRole POST: Identity role validation failed. UserId={UserId}, AssignmentId={AssignmentId}, RoleId={RoleId}, RoleFound={RoleFound}",
                        user.Id,
                        assignment.AssignmentId,
                        assignment.RoleId,
                        role != null);

                    return Forbid();
                }

                _logger.LogInformation(
                    "SelectRole POST: Role validated. UserId={UserId}, RoleName={RoleName}, AssignmentId={AssignmentId}",
                    user.Id,
                    role.Name,
                    assignment.AssignmentId);

                _logger.LogInformation(
                    "SelectRole POST: Calling SetWorkingRole. TraceId={TraceId}",
                    traceId);

                return await SetWorkingRole(assignment.AssignmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SelectRole POST failed. TraceId={TraceId}, AssignmentId={AssignmentId}",
                    traceId,
                    model.AssignmentId);

                throw;
            }
        }


        private async Task<IActionResult> ReloadRoleSelection(
            string userId,
            SelectRoleViewModel model)
        {
            var traceId = HttpContext.TraceIdentifier;

            _logger.LogInformation(
                "ReloadRoleSelection started. TraceId={TraceId}, UserId={UserId}, AssignmentId={AssignmentId}",
                traceId,
                userId,
                model.AssignmentId);

            try
            {
                var assignments = await _context.CijUserRoleAssignments
                    .Where(x => x.UserId == userId && x.IsActive)
                    .Join(
                        _context.Roles,
                        a => a.RoleId,
                        r => r.Id,
                        (a, r) => new
                        {
                            Assignment = a,
                            Role = r
                        })
                    .ToListAsync();

                _logger.LogInformation(
                    "ReloadRoleSelection: Active assignments loaded. UserId={UserId}, Count={Count}",
                    userId,
                    assignments.Count);

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning(
                        "ReloadRoleSelection: User not found. UserId={UserId}, TraceId={TraceId}",
                        userId,
                        traceId);

                    return Challenge();
                }

                var identityRoles = await _userManager.GetRolesAsync(user);

                _logger.LogInformation(
                    "ReloadRoleSelection: Identity roles loaded. UserId={UserId}, RoleCount={RoleCount}, Roles={Roles}",
                    userId,
                    identityRoles.Count,
                    string.Join(", ", identityRoles));

                var assignmentCountBeforeFilter = assignments.Count;

                assignments = assignments
                    .Where(x => identityRoles.Contains(x.Role.Name!))
                    .ToList();

                _logger.LogInformation(
                    "ReloadRoleSelection: Role filtering completed. BeforeFilter={BeforeFilter}, AfterFilter={AfterFilter}",
                    assignmentCountBeforeFilter,
                    assignments.Count);

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

                _logger.LogInformation(
                    "ReloadRoleSelection: Loading lookup data. DepartmentCount={DepartmentCount}, LocationCount={LocationCount}",
                    departmentIds.Count,
                    locationIds.Count);

                var departments = await _context.CijDepartments
                    .Where(x => departmentIds.Contains(x.DepartmentId))
                    .ToDictionaryAsync(
                        x => x.DepartmentId,
                        x => x.DepartmentName);

                var locations = await _context.CijLocations
                    .Where(x => locationIds.Contains(x.LocationId))
                    .ToDictionaryAsync(
                        x => x.LocationId,
                        x => x.LocationName);

                _logger.LogInformation(
                    "ReloadRoleSelection: Lookup data loaded. Departments={DepartmentCount}, Locations={LocationCount}",
                    departments.Count,
                    locations.Count);

                model.Roles = assignments.Select(x =>
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
                                a.DepartmentId.Value,
                                "Unknown Department")
                            : "All Departments",
                        LocationId = a.LocationId,
                        LocationName = a.LocationId.HasValue
                            ? locations.GetValueOrDefault(
                                a.LocationId.Value,
                                "Unknown Location")
                            : "All Locations",
                        ScopeType = a.ScopeType ?? string.Empty
                    };
                }).ToList();

                _logger.LogInformation(
                    "ReloadRoleSelection completed. UserId={UserId}, ViewRoleCount={ViewRoleCount}, TraceId={TraceId}",
                    userId,
                    model.Roles.Count,
                    traceId);

                return View("SelectRole", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ReloadRoleSelection failed. UserId={UserId}, AssignmentId={AssignmentId}, TraceId={TraceId}",
                    userId,
                    model.AssignmentId,
                    traceId);

                throw;
            }
        }


        private async Task<IActionResult> SetWorkingRole(int assignmentId)
        {
            var traceId = HttpContext.TraceIdentifier;

            _logger.LogInformation(
                "SetWorkingRole started. TraceId={TraceId}, AssignmentId={AssignmentId}, IsAuthenticated={IsAuthenticated}",
                traceId,
                assignmentId,
                User.Identity?.IsAuthenticated);

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogWarning(
                        "SetWorkingRole: User not found. Returning Challenge. TraceId={TraceId}",
                        traceId);

                    return Challenge();
                }

                _logger.LogInformation(
                    "SetWorkingRole: User found. UserId={UserId}, Email={Email}",
                    user.Id,
                    user.Email);

                var assignment = await _context.CijUserRoleAssignments
                    .FirstOrDefaultAsync(x =>
                        x.AssignmentId == assignmentId &&
                        x.UserId == user.Id &&
                        x.IsActive);

                if (assignment == null)
                {
                    _logger.LogWarning(
                        "SetWorkingRole: Assignment not found, inactive, or not owned by user. UserId={UserId}, AssignmentId={AssignmentId}",
                        user.Id,
                        assignmentId);

                    return Forbid();
                }

                _logger.LogInformation(
                    "SetWorkingRole: Assignment validated. AssignmentId={AssignmentId}, RoleId={RoleId}, DepartmentId={DepartmentId}, LocationId={LocationId}",
                    assignment.AssignmentId,
                    assignment.RoleId,
                    assignment.DepartmentId,
                    assignment.LocationId);

                var role = await _context.Roles
                    .FirstOrDefaultAsync(x => x.Id == assignment.RoleId);

                var identityRoles = await _userManager.GetRolesAsync(user);

                if (role?.Name == null ||
                    !identityRoles.Contains(role.Name))
                {
                    _logger.LogWarning(
                        "SetWorkingRole: Role validation failed. UserId={UserId}, AssignmentId={AssignmentId}, RoleId={RoleId}, RoleFound={RoleFound}",
                        user.Id,
                        assignmentId,
                        assignment.RoleId,
                        role != null);

                    return Forbid();
                }

                _logger.LogInformation(
                    "SetWorkingRole: Role validated. RoleName={RoleName}, AssignmentId={AssignmentId}",
                    role.Name,
                    assignment.AssignmentId);

                // Store only the validated, selected working context.
                HttpContext.Session.SetString(
                    "WorkingAssignmentId",
                    assignment.AssignmentId.ToString());

                HttpContext.Session.SetString(
                    "WorkingRoleId",
                    assignment.RoleId);

                HttpContext.Session.SetString(
                    "WorkingRoleName",
                    role.Name);

                if (assignment.DepartmentId.HasValue)
                {
                    HttpContext.Session.SetInt32(
                        "WorkingDepartmentId",
                        assignment.DepartmentId.Value);
                }
                else
                {
                    HttpContext.Session.Remove("WorkingDepartmentId");
                }

                if (assignment.LocationId.HasValue)
                {
                    HttpContext.Session.SetInt32(
                        "WorkingLocationId",
                        assignment.LocationId.Value);
                }
                else
                {
                    HttpContext.Session.Remove("WorkingLocationId");
                }

                _logger.LogInformation(
                    "SetWorkingRole: Session values set. AssignmentId={AssignmentId}, RoleId={RoleId}, RoleName={RoleName}, DepartmentId={DepartmentId}, LocationId={LocationId}, SessionId={SessionId}",
                    assignment.AssignmentId,
                    assignment.RoleId,
                    role.Name,
                    assignment.DepartmentId,
                    assignment.LocationId,
                    HttpContext.Session.Id);

                // Read values back to confirm they are available in this request.
                _logger.LogInformation(
                    "SetWorkingRole: Session verification. WorkingAssignmentId={WorkingAssignmentId}, WorkingRoleId={WorkingRoleId}, WorkingRoleName={WorkingRoleName}",
                    HttpContext.Session.GetString("WorkingAssignmentId"),
                    HttpContext.Session.GetString("WorkingRoleId"),
                    HttpContext.Session.GetString("WorkingRoleName"));

                _logger.LogInformation(
                    "SetWorkingRole: Redirecting to CIJ/Dashboard. TraceId={TraceId}",
                    traceId);

                return RedirectToAction("Dashboard", "CIJ");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SetWorkingRole failed. AssignmentId={AssignmentId}, TraceId={TraceId}",
                    assignmentId,
                    traceId);

                throw;
            }
        }

    }
}
