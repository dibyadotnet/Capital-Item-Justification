using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Capital_Item_Justification.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Capital_Item_Justification.Controllers
{
    public class CIJController : Controller
    {
        private readonly ICIJRequestService _service;
        public CIJController(ICIJRequestService service)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CreateCIJ()
        {
            CIJMainViewModel vm = new();
            try
            {
                var itemTypes = await _service.GetItemType();
                var itemTypesItem = itemTypes.Select(x => new SelectListItem
                {
                    Value = x.ItemTypeId.ToString(),
                    Text = x.ItemTypeCode
                }).ToList();

                var BudgetProvisionItem = new List<SelectListItem>()
                {
                    new SelectListItem { Value="Yes",Text="Yes"},
                    new SelectListItem { Value="No",Text="No"}
                };
                var PurchasePurposeItem = new List<SelectListItem>()
                {
                    new SelectListItem { Value="New Requirement",Text="New Requirement"},
                    new SelectListItem { Value="Replacement",Text="Replacement"}
                };
                var TreatmentOldEqupItem = new List<SelectListItem>()
                {
                    new SelectListItem { Value="Scraped",Text="Scraped"},
                    new SelectListItem { Value="Unde buy back",Text="Unde buy back"}
                };


                var requestViewModel = new CIJRequestViewModel
                {
                    ItemTypes = itemTypesItem,
                    BudgetProvisionList = BudgetProvisionItem
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
    }
}
