using Capital_Item_Justification.Models;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Capital_Item_Justification.Controllers
{
    public class WorkFlowController : Controller
    {
        private readonly ILogger<WorkFlowController> _logger;
        private readonly IWorkflowService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        public WorkFlowController(ILogger<WorkFlowController> logger, IWorkflowService service, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _service = service;
            _userManager = userManager;
        }
        public IActionResult MyApproval()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();
                string roleid = string.Empty;
                _service.GetMyApproval(userId, roleid);
            }
            catch (Exception)
            {

                throw;
            }
           
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SubmitRequest(CIJMainViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var roles = await _userManager.GetRolesAsync(user);
             
                await _service.SubmitCIJAsync(model, user?.Id);
                TempData["SuccessMessage"] = "CIJ request submitted successfully.";

                return RedirectToAction("Dashboard", "CIJ");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Unable to submit the CIJ request.";
                return RedirectToAction("Dashboard", "CIJ");
            }
        }
    }
}
