using Capital_Item_Justification.Services;
using Capital_Item_Justification.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Capital_Item_Justification.Controllers
{
    public class GeneratePDFController : Controller
    {
        private readonly ICIJPdfService _cIJPdfService;
        private readonly ICIJRequestService _cijService;
        public GeneratePDFController(ICIJPdfService cIJPdfService, ICIJRequestService cijService)
        {
            _cIJPdfService = cIJPdfService;
            _cijService= cijService;
        }
        public async Task<IActionResult> DownloadCIJPDF(int cijId)
        {
            try
            {
                var pdfBytes = await _cIJPdfService.GeneratePdfAsync(cijId);

                var cij = await _cijService.GetCIJById(cijId);
                if (cij == null) 
                    return NotFound();
                string fileName = $"{cij.CIJRequest.CIJSNumber.Replace("/", "-")}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                // Log exception
                return BadRequest("Unable to generate CIJ PDF.");
            }
        }
    }
}
