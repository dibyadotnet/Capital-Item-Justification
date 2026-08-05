using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Capital_Item_Justification.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Capital_Item_Justification.Controllers
{
    public class CIJController : Controller
    {
        private readonly ICIJRequestService _service;
        public CIJController(ICIJRequestService service)
        {
            _service = service;
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
                    OldEqupTreatmentList = dropDowns.OldEqupTreatmentList
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
                if (cIJMainViewModel.CIJRequest.Cijid > 0)
                {
                    await _service.UpdateCIJ(cIJMainViewModel);
                }
                else
                {
                    if (!string.IsNullOrEmpty(cIJMainViewModel.EquipmentJson))
                    {
                        cIJMainViewModel.Equipments = JsonSerializer.Deserialize<List<CIJEquipmentViewModel>>(cIJMainViewModel.EquipmentJson);
                    }
                    await _service.SaveCIJ(cIJMainViewModel);
                }
                return RedirectToAction("Dashboard", "CIJ");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(cIJMainViewModel);
            }
        }

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
                CIJRequestViewModel dropDowns = await PopulateDropDownList();

                cIJMainViewModel.CIJRequest.ItemTypes = dropDowns.ItemTypes;
                cIJMainViewModel.CIJRequest.Departments = dropDowns.Departments;
                cIJMainViewModel.CIJRequest.Locations = dropDowns.Locations;
                cIJMainViewModel.CIJRequest.BudgetProvisionList = dropDowns.BudgetProvisionList;
                cIJMainViewModel.CIJRequest.PurchagePurposeList = dropDowns.PurchagePurposeList;
                cIJMainViewModel.CIJRequest.OldEqupTreatmentList = dropDowns.OldEqupTreatmentList;

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

                return new CIJRequestViewModel
                {
                    ItemTypes = itemTypesItem,
                    Departments = departmentItems,
                    Locations = locationItems,
                    BudgetProvisionList = BudgetProvisionItem,
                    PurchagePurposeList = PurchasePurposeItem,
                    OldEqupTreatmentList = oldEqupTreatmentItem
                };
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
