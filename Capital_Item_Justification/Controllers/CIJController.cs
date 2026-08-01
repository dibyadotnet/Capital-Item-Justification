using Microsoft.AspNetCore.Mvc;

namespace Capital_Item_Justification.Controllers
{
    public class CIJController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CreateCIJ()
        {
            return View();
        }
    }
}
