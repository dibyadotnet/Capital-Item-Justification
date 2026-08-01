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
                var requestViewModel = new CIJRequestViewModel
                {
                    ItemTypes = itemTypes.Select(x => new SelectListItem
                    {
                        Value = x.ItemTypeId.ToString(),
                        Text = x.ItemTypeName
                    }).ToList()
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
