using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Capital_Item_Justification.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Capital_Item_Justification.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Capital_Item_Justification.Controllers
{
    [Authorize]
    public class CIJController : Controller
    {
        private readonly ICIJRequestService _service;
        private readonly ILogger<WorkFlowController> _logger;
        private readonly IWorkflowService _workflowService;
        private readonly UserManager<ApplicationUser> _userManager;
        public CIJController(ICIJRequestService service, ILogger<WorkFlowController> logger, IWorkflowService workflowService, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _logger = logger;
            _workflowService = workflowService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Dashboard()
        {
            List<DashboardViewModel> list = new();
            try
            {
                list = await _service.GetDashboard();
            }
            catch (Exception)
            {

                throw;
            }
            return View(list);
        }
        [Authorize(Roles = "Requester")]
        public async Task<IActionResult> CreateCIJ()
        {
            CIJMainViewModel vm = new();
            try
            {
                CIJRequestViewModel dropDowns = await PopulateDropDownList();

                var requestViewModel = new CIJRequestViewModel
                {
                    ItemTypes = dropDowns.ItemTypes,
                    Departments = dropDowns.Departments,
                    Locations = dropDowns.Locations,
                    BudgetProvisionList = dropDowns.BudgetProvisionList,
                    PurchagePurposeList = dropDowns.PurchagePurposeList,
                    OldEqupTreatmentList = dropDowns.OldEqupTreatmentList,
                    ProjectList = dropDowns.ProjectList,
                    CostCenterList = dropDowns.CostCenterList,
                    BudgetTypeList = dropDowns.BudgetTypeList,
                };

                vm = new CIJMainViewModel
                {
                    CIJRequest = requestViewModel,
                    Equipments = new List<CIJEquipmentViewModel>(),
                    Vendors = new List<CIJVendorViewModel>(),
                    Justification = new CIJJustificationViewModel()
                };
            }
            catch (Exception)
            {
                throw;
            }

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> SaveCIJ(CIJMainViewModel cIJMainViewModel)
        {
            try
            {
                string beneficieryDept = string.Empty;
                string beneficieryLoc = string.Empty;
                int cijId = 0;

                var selectedBenefDepts = cIJMainViewModel?.CIJRequest?.SelectedBenefDeptIds;
                var selectedBenefLocs = cIJMainViewModel?.CIJRequest?.SelectedBenefLocIds;
                if (selectedBenefDepts != null && selectedBenefDepts.Count > 0)
                {
                    beneficieryDept = string.Join(",", selectedBenefDepts);
                }
                if (selectedBenefLocs != null && selectedBenefLocs.Count > 0)
                {
                    beneficieryLoc = string.Join(",", selectedBenefLocs);
                }
                cIJMainViewModel.CIJRequest.BeneficiaryDepartment = beneficieryDept;
                cIJMainViewModel.CIJRequest.BeneficiaryLocation = beneficieryLoc;
                if (cIJMainViewModel?.CIJRequest?.Cijid > 0)
                {
                    cijId = cIJMainViewModel.CIJRequest.Cijid;
                    if (!string.IsNullOrEmpty(cIJMainViewModel.EquipmentJson))
                    {
                        List<CIJEquipmentViewModel>? EquipmentsJson = JsonSerializer.Deserialize<List<CIJEquipmentViewModel>>(cIJMainViewModel.EquipmentJson);
                        cIJMainViewModel.Equipments = EquipmentsJson;
                    }
                    await _service.UpdateCIJ(cIJMainViewModel);
                    TempData["ToastMessage"] = "CIJ updated successfully.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    int? locationId = cIJMainViewModel.CIJRequest.CostCenterId;
                    string cijNumber = await _service.GenerateCIJNumber(locationId);
                    cIJMainViewModel.CIJRequest.CIJSNumber = cijNumber;
                    if (!string.IsNullOrEmpty(cIJMainViewModel.EquipmentJson))
                    {
                        cIJMainViewModel.Equipments = JsonSerializer.Deserialize<List<CIJEquipmentViewModel>>(cIJMainViewModel.EquipmentJson);
                    }
                    cijId = await _service.SaveCIJ(cIJMainViewModel);
                    TempData["ToastMessage"] = "CIJ saved successfully.";
                    TempData["ToastType"] = "success";
                }
                return RedirectToAction(nameof(Edit), new { cijId = cijId });
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Error while Save CIJ.";
                TempData["ToastType"] = "error";
                if (cIJMainViewModel.CIJRequest.Cijid>0) {
                    return RedirectToAction(nameof(Edit), new { cijId = cIJMainViewModel.CIJRequest.Cijid });
                }
                else
                {
                    return View("CreateCIJ", cIJMainViewModel);
                }
            }
        }

        [Authorize(Roles = "Requester")]
        [HttpGet]
        public async Task<IActionResult> Edit(int cijId)
        {
            try
            {
                if (cijId <= 0)
                {
                    return BadRequest();
                }

                var cIJMainViewModel = await _service.GetCIJById(cijId);

                if (cIJMainViewModel == null)
                {
                    return NotFound();
                }
                if (cIJMainViewModel.CIJRequest != null)
                {
                    if (!string.IsNullOrEmpty(cIJMainViewModel.CIJRequest.BeneficiaryDepartment))
                    {
                        cIJMainViewModel.CIJRequest.SelectedBenefDeptIds = cIJMainViewModel.CIJRequest.BeneficiaryDepartment
                                                                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                                            .Select(int.Parse)
                                                                            .ToList();
                    }
                    if (!string.IsNullOrEmpty(cIJMainViewModel.CIJRequest.BeneficiaryLocation))
                    {
                        cIJMainViewModel.CIJRequest.SelectedBenefLocIds = cIJMainViewModel.CIJRequest.BeneficiaryLocation
                                                                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                                            .Select(int.Parse)
                                                                            .ToList();
                    }
                }

                CIJRequestViewModel dropDowns = await PopulateDropDownList();

                cIJMainViewModel.CIJRequest.ItemTypes = dropDowns.ItemTypes;
                cIJMainViewModel.CIJRequest.Departments = dropDowns.Departments;
                cIJMainViewModel.CIJRequest.Locations = dropDowns.Locations;
                cIJMainViewModel.CIJRequest.BudgetProvisionList = dropDowns.BudgetProvisionList;
                cIJMainViewModel.CIJRequest.PurchagePurposeList = dropDowns.PurchagePurposeList;
                cIJMainViewModel.CIJRequest.OldEqupTreatmentList = dropDowns.OldEqupTreatmentList;
                cIJMainViewModel.CIJRequest.ProjectList = dropDowns.ProjectList;
                cIJMainViewModel.CIJRequest.CostCenterList = dropDowns.CostCenterList;
                cIJMainViewModel.CIJRequest.BudgetTypeList = dropDowns.BudgetTypeList;

                var culture = new CultureInfo("en-IN");
                string formattedCost = string.Format(culture, "₹ {0:N2}", cIJMainViewModel.CIJRequest.TotalEquipmentCost);
                cIJMainViewModel.CIJRequest.TotalEquipmentCostDisplay = formattedCost;

                @ViewBag.TotalEstEquipmentCost = formattedCost;

                //get attachments
                List<AttachmentViewModel> model = await _service.GetAttachmentsById(cijId);
                cIJMainViewModel.AttachmentVm = model;

                return View("CreateCIJ", cIJMainViewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(new CIJMainViewModel());
            }
        }


        private async Task<CIJRequestViewModel> PopulateDropDownList()
        {
            try
            {
                //Item Type Master
                var itemTypes = await _service.GetItemType();
                var itemTypesItem = itemTypes.Select(x => new SelectListItem
                {
                    Value = x.ItemTypeId.ToString(),
                    Text = x.ItemTypeCode
                }).ToList();

                //Department master
                var departmentsList = await _service.GetDepartment();
                var departmentItems = departmentsList.Select(x => new SelectListItem
                {
                    Value = x.DepartmentId.ToString(),
                    Text = x.DepartmentName
                }).ToList();

                //Location master
                var locationList = await _service.GetLocation();
                var locationItems = locationList.Select(x => new SelectListItem
                {
                    Value = x.LocationId.ToString(),
                    Text = x.LocationName
                }).ToList();


                var BudgetProvisionItem = new List<SelectListItem>()
                {
                    new SelectListItem { Value="Yes",Text="Yes"},
                    new SelectListItem { Value="No",Text="No"}
                };
                var PurchasePurposeItems = await _service.GetPurchasePurpose();
                var PurchasePurposeItem = PurchasePurposeItems.Select(x => new SelectListItem
                {
                    Value = x.PurposeId.ToString(),
                    Text = x.PurposeName
                }).ToList();

                var TreatmentItems = await _service.GetOldEquipmentTreatment();
                var oldEqupTreatmentItem = TreatmentItems.Select(x => new SelectListItem
                {
                    Value = x.TreatmentId.ToString(),
                    Text = x.TreatmentName
                }).ToList();

                //Project Master
                var projects = await _service.GetProjectCode();
                var projectItems = projects.Select(x => new SelectListItem
                {
                    Value = x.ProjectId.ToString(),
                    Text = x.ProjectCode
                }).ToList();

                //Cost Center
                var costCenters = await _service.GetCostCenter();
                var costCenterItem = costCenters.Select(x => new SelectListItem
                {
                    Value = x.CostCenterId.ToString(),
                    Text = x.CostCenterName
                }).ToList();

                var budgetTypes = await _service.GetBudgetType();
                var budgetItem = budgetTypes.Select(x => new SelectListItem
                {
                    Value = x.BudgetTypeId.ToString(),
                    Text = x.BudgetTypeName
                }).ToList();

                return new CIJRequestViewModel
                {
                    ItemTypes = itemTypesItem,
                    Departments = departmentItems,
                    Locations = locationItems,
                    BudgetProvisionList = BudgetProvisionItem,
                    PurchagePurposeList = PurchasePurposeItem,
                    OldEqupTreatmentList = oldEqupTreatmentItem,
                    ProjectList = projectItems,
                    CostCenterList = costCenterItem,
                    BudgetTypeList = budgetItem
                };
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(int attachmentId, int cijId)
        {
            try
            {
                var attachments = await _service.GetAttachmentsById(cijId);
                if (attachments == null && attachments?.Count == 0)
                    return NotFound("Attachment file not found.");

                var attachment = attachments?.Where(x => x.AttachmentId == attachmentId).FirstOrDefault();

                if (attachment == null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(attachment.FilePath) ||
                    !System.IO.File.Exists(attachment.FilePath))
                {
                    return NotFound("Attachment file not found.");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(attachment.FilePath);

                return File(
                    fileBytes,
                    "application/octet-stream",
                    attachment.FileName
                );
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            try
            {
                int deleteStatus = await _service.DeleteAttachment(attachmentId);
                if (deleteStatus == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Attachment not found."
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = true,
                        message = "Attachment deleted successfully."
                    });
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public async Task<IActionResult> SubmitRequest(CIJMainViewModel cIJMainViewModel)
        {
            try
            {
                var formData = Request.Form
        .ToDictionary(x => x.Key, x => x.Value.ToString());

             
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var roles = await _userManager.GetRolesAsync(user);

                await _workflowService.SubmitCIJAsync(cIJMainViewModel, user?.Id);
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
