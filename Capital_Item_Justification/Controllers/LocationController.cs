using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Capital_Item_Justification.Controllers
{
    public class LocationController : Controller
    {
        private readonly ILocationService _service;
        public LocationController(ILocationService service)
        {
            _service = service;
        }
        public async Task<IActionResult> GetAllLocation()
        {
            List<LocationViewModel> locations = new List<LocationViewModel>();
            try
            {
                locations = await _service.GetAllAsync();
            }
            catch (Exception)
            {

                throw;
            }
            return View(locations);
        }
        [HttpGet]
        public IActionResult CreateLocation()
        {
            try
            {
                LocationViewModel locationViewModel = new LocationViewModel();
                var locationTypeItem = new List<SelectListItem>()
                {
                    new SelectListItem { Value="Primary",Text="Primary"},
                    new SelectListItem { Value="Secondary",Text="Secondary"}
                };
                locationViewModel.LocationTypeList = locationTypeItem;

                return View(locationViewModel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        //[HttpPost]
        //public async Task<IActionResult> Save(LocationViewModel model)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
    }
}
