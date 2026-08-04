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


                var requestViewModel = new CIJRequestViewModel
                {
                    ItemTypes = itemTypesItem,
                    Departments=departmentItems,
                    Locations=locationItems,
                    BudgetProvisionList = BudgetProvisionItem,
                    PurchagePurposeList = PurchasePurposeItem,
                    OldEqupTreatmentList = oldEqupTreatmentItem
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
                if (!string.IsNullOrEmpty(cIJMainViewModel.EquipmentJson))
                {
                    cIJMainViewModel.Equipments = JsonSerializer.Deserialize<List<CIJEquipmentViewModel>>(cIJMainViewModel.EquipmentJson);
                }
                await _service.SaveCIJ(cIJMainViewModel);
                return View(cIJMainViewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(cIJMainViewModel);
                throw;
            }
        }
    }
}
