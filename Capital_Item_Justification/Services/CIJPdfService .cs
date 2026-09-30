using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.ViewModels;
using Microsoft.Extensions.Hosting;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Globalization;

namespace Capital_Item_Justification.Services
{
    public class CIJPdfService : ICIJPdfService
    {
        private readonly IWorkflowRepository _workflowRepository;

        private readonly XFont _titleFont;
        private readonly XFont _sectionFont;
        private readonly XFont _normalFont;
        private readonly XFont _boldFont;
        private readonly XFont _smallFont;

        private const double PageWidth = 595;
        private const double PageHeight = 842;
        private const double MarginLeft = 40;
        private const double MarginRight = 40;
        private const double MarginTop = 40;
        private const double MarginBottom = 45;
        public CIJPdfService(IWorkflowRepository workflowRepository)
        {
            _workflowRepository = workflowRepository;
            _titleFont = new XFont("Arial", 18, XFontStyleEx.Bold);
            _sectionFont = new XFont("Arial", 11, XFontStyleEx.Bold);
            _normalFont = new XFont("Arial", 9, XFontStyleEx.Regular);
            _boldFont = new XFont("Arial", 9, XFontStyleEx.Bold);
            _smallFont = new XFont("Arial", 8, XFontStyleEx.Regular);
        }
        public async Task<byte[]> GeneratePdfAsync(int cijId)
        {
            var model = await GetPdfDataAsync(cijId, 0);

            if (model == null)
                throw new Exception("CIJ request not found.");

            using var stream = new MemoryStream();

            var document = new PdfDocument();

            //document.Info.Title = $"CIJ - {model.CIJNumber}";

            document.Info.Author =
                "CIJ System";

            BuildDocument(document, model);

            document.Save(stream, false);

            return stream.ToArray();
        }

        private async Task<CIJPdfViewModel?> GetPdfDataAsync(int cijId, int approvalId)
        {
            var cijdetails = await _workflowRepository.GetRequestDetailsAsync(approvalId, cijId);
            if (cijdetails == null)
            {
                return null;
            }
            CIJPdfViewModel cIJPdfViewModel = new CIJPdfViewModel()
            {
                CIJRequest = cijdetails.CIJRequest,
                cIJEquipmentViewModels = cijdetails.cIJEquipmentViewModels,
                cIJJustificationViewModel = cijdetails.cIJJustificationViewModel,
                committeeCommentViewModel = cijdetails.committeeCommentViewModel,
                attachmentViewModels = cijdetails.attachmentViewModels,
                workflowHistory = cijdetails.workflowHistory,
                Clarifications = cijdetails.Clarifications
            };

            return cIJPdfViewModel;
        }
        private void BuildDocument(PdfDocument document, CIJPdfViewModel model)
        {
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);
            double y = MarginTop;
            int pageNumber = 1;

            DrawHeader(gfx, model, ref y);

            DrawRequestInformation(gfx, page, model, ref y);

            DrawEquipmentTable(document, gfx, page, model, ref y, ref pageNumber);

            DrawJustification(document, gfx, page, model, ref y, ref pageNumber);

            DrawApprovalHistory(document, gfx, page, model, ref y, ref pageNumber);

            //DrawFooter(gfx, page, model, pageNumber);
        }
        private void DrawHeader(XGraphics gfx, CIJPdfViewModel model, ref double y)
        {
            var darkBlue = XColor.FromArgb(31, 78, 121);
            var lightBlue = XColor.FromArgb(221, 235, 247);
            var headerBrush = new XSolidBrush(darkBlue);
            gfx.DrawRectangle(headerBrush, MarginLeft, y, PageWidth - MarginLeft - MarginRight, 55);

            gfx.DrawString("CAPITAL ITEM JUSTIFICATION", _titleFont, XBrushes.White, new XRect(MarginLeft + 15, y + 8, PageWidth - 120, 25), XStringFormats.TopLeft);

            gfx.DrawString(model.CIJRequest.CIJSNumber, _boldFont, XBrushes.White, new XRect(PageWidth - 190, y + 20, 135, 20), XStringFormats.TopRight);

            y += 65;

            gfx.DrawString("CIJ REQUEST", _sectionFont, new XSolidBrush(darkBlue), new XRect(MarginLeft, y, 200, 20), XStringFormats.TopLeft);

            y += 25;
        }
        private void DrawRequestInformation(XGraphics gfx, PdfPage page, CIJPdfViewModel model, ref double y)
        {
            const double rowHeight = 25;

            double tableWidth =
                PageWidth - MarginLeft - MarginRight;

            double labelWidth = 115;

            var borderPen =
                new XPen(XColors.LightGray, 0.7);

            var labelBrush =
                new XSolidBrush(
                    XColor.FromArgb(242, 242, 242));

            DrawInfoRow(
                gfx,
                ref y,
                "CIJ Number",
                model.CIJRequest.CIJSNumber,
                "Request Date",
                model.CIJRequest.RequestDate.ToString("dd-MMM-yyyy") ?? "",
                labelWidth,
                tableWidth,
                rowHeight,
                borderPen,
                labelBrush);

            DrawInfoRow(
                gfx,
                ref y,
                "Requester",
                model.CIJRequest.RequestorName,
                "Employee Code",
                model.CIJRequest.RequestorName,
                labelWidth,
                tableWidth,
                rowHeight,
                borderPen,
                labelBrush);

            DrawInfoRow(
                gfx,
                ref y,
                "Department",
                model.CIJRequest.RequestDepartment,
                "Location",
                model.CIJRequest.LocationName,
                labelWidth,
                tableWidth,
                rowHeight,
                borderPen,
                labelBrush);

            DrawInfoRow(
                gfx,
                ref y,
                "Cost Center",
                model.CIJRequest.CostCenterName,
                "Item Type",
                model.CIJRequest.ItemTypeName,
                labelWidth,
                tableWidth,
                rowHeight,
                borderPen,
                labelBrush);

            DrawInfoRow(
                gfx,
                ref y,
                "Budget Provision",
                model.CIJRequest.BudgetProvision,
                "Status",
                model.CIJRequest.StatusName,
                labelWidth,
                tableWidth,
                rowHeight,
                borderPen,
                labelBrush);

            DrawInfoRow(
                gfx,
                ref y,
                "Project",
                model.CIJRequest.ProjectCode,
                "Project Fund",
                model.CIJRequest.ProjectFund,
                labelWidth,
                tableWidth,
                rowHeight,
                borderPen,
                labelBrush);

            DrawInfoRow(
                gfx,
                ref y,
                "Benef Department",
                model.CIJRequest.BeneficiaryDepartment,
                "Benef Location",
                model.CIJRequest.BeneficiaryLocation,
                labelWidth,
                tableWidth,
                rowHeight,
            borderPen,
                labelBrush);
            string projectCostText = "₹" + model.CIJRequest.ProjectCost?.ToString("N2", new CultureInfo("en-IN"));
            DrawInfoRow(
                gfx,
                ref y,
                "Project Cost",
                "₹" + model.CIJRequest.ProjectCost?.ToString("N0"),
                "SCEH Cost",
                "₹" + model.CIJRequest.Scehcost?.ToString("N0"),
                labelWidth,
                tableWidth,
                rowHeight,
                borderPen,
                labelBrush);
            DrawInfoRow(
                gfx,
                ref y,
                "Equipment Cost",
                "₹" + model.CIJRequest.TotalEquipmentCost?.ToString("N0"),
                "",
                "",
                labelWidth,
                tableWidth,
                rowHeight,
                borderPen,
                labelBrush);

            y += 15;
        }
        private void DrawInfoRow(XGraphics gfx, ref double y, string label1, string value1, string label2, string value2,
                                double labelWidth, double tableWidth, double rowHeight, XPen borderPen, XBrush labelBrush)
        {
            double halfWidth = tableWidth / 2;

            double valueWidth =
                halfWidth - labelWidth;

            // First label
            gfx.DrawRectangle(
                labelBrush,
                MarginLeft,
                y,
                labelWidth,
                rowHeight);

            gfx.DrawRectangle(
                borderPen,
                MarginLeft,
                y,
                labelWidth,
                rowHeight);

            gfx.DrawString(
                label1,
                _boldFont,
                XBrushes.Black,
                new XRect(
                    MarginLeft + 5,
                    y + 6,
                    labelWidth - 10,
                    rowHeight),
                XStringFormats.TopLeft);

            // First value
            gfx.DrawRectangle(
                borderPen,
                MarginLeft + labelWidth,
                y,
                valueWidth,
                rowHeight);

            gfx.DrawString(
                value1 ?? "",
                _normalFont,
                XBrushes.Black,
                new XRect(
                    MarginLeft + labelWidth + 5,
                    y + 6,
                    valueWidth - 10,
                    rowHeight),
                XStringFormats.TopLeft);

            double secondX =
                MarginLeft + halfWidth;

            // Second label
            gfx.DrawRectangle(
                labelBrush,
                secondX,
                y,
                labelWidth,
                rowHeight);

            gfx.DrawRectangle(
                borderPen,
                secondX,
                y,
                labelWidth,
                rowHeight);

            gfx.DrawString(
                label2,
                _boldFont,
                XBrushes.Black,
                new XRect(
                    secondX + 5,
                    y + 6,
                    labelWidth - 10,
                    rowHeight),
                XStringFormats.TopLeft);

            // Second value
            gfx.DrawRectangle(
                borderPen,
                secondX + labelWidth,
                y,
                valueWidth,
                rowHeight);

            gfx.DrawString(
                value2 ?? "",
                _normalFont,
                XBrushes.Black,
                new XRect(
                    secondX + labelWidth + 5,
                    y + 6,
                    valueWidth - 10,
                    rowHeight),
                XStringFormats.TopLeft);

            y += rowHeight;
        }

        private void DrawEquipmentTable(PdfDocument document, XGraphics gfx, PdfPage page, CIJPdfViewModel model, ref double y, ref int pageNumber)
        {
            y += 10;
            DrawSectionTitle(gfx, "EQUIPMENT DETAILS", ref y);

            double[] widths = { 40, 140, 100, 80, 55, 100 };//510

            string[] headers =
            {
        "S.No",
        "Equipment Name",
        "Manufacturer",
        "Model",
        "Quantity",
        "Estimated Cost"
    };
            DrawTableHeader(gfx, headers, widths, ref y);
            decimal totalEquipmentCost = 0;
            int slNo = 1;
            foreach (var item in model.cIJEquipmentViewModels)
            {
                item.SlNo = slNo++;
                totalEquipmentCost += item.EquipmentCost;
                if (y > PageHeight - 100)
                {
                    //DrawFooter(gfx, page, model, pageNumber);

                    page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A4;

                    gfx = XGraphics.FromPdfPage(page);
                    pageNumber++;
                    y = MarginTop;

                    DrawSectionTitle(gfx, "EQUIPMENT DETAILS - CONTINUED", ref y);

                    DrawTableHeader(gfx, headers, widths, ref y);
                }

                DrawEquipmentRow(gfx, item, widths, ref y);
            }
            if (y > PageHeight - MarginBottom - 70)
            {
                //DrawFooter(gfx, page, model, pageNumber);

                page = document.AddPage();

                page.Size =
                    PdfSharp.PageSize.A4;

                gfx =
                    XGraphics.FromPdfPage(page);

                pageNumber++;

                y = MarginTop;
            }
            DrawTotalRow(gfx, totalEquipmentCost, widths, ref y);
        }
        private void DrawSectionTitle(XGraphics gfx, string title, ref double y)
        {
            var brush =
                new XSolidBrush(
                    XColor.FromArgb(31, 78, 121));

            gfx.DrawRectangle(
                brush,
                MarginLeft,
                y,
                PageWidth - MarginLeft - MarginRight,
                25);

            gfx.DrawString(
                title,
                _boldFont,
                XBrushes.White,
                new XRect(
                    MarginLeft + 8,
                    y + 6,
                    PageWidth - MarginLeft - MarginRight - 16,
                    20),
                XStringFormats.TopLeft);

            y += 30;
        }
        private void DrawTableHeader(XGraphics gfx, string[] headers, double[] widths, ref double y)
        {
            double x = MarginLeft;

            double height = 28;

            var headerBrush =
                new XSolidBrush(
                    XColor.FromArgb(31, 78, 121));

            var border =
                new XPen(XColors.White, 0.7);

            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(
                    headerBrush,
                    x,
                    y,
                    widths[i],
                    height);

                gfx.DrawRectangle(
                    border,
                    x,
                    y,
                    widths[i],
                    height);

                gfx.DrawString(
                    headers[i],
                    _boldFont,
                    XBrushes.White,
                    new XRect(
                        x + 3,
                        y + 7,
                        widths[i] - 6,
                        height),
                    XStringFormats.TopCenter);

                x += widths[i];
            }

            y += height;
        }

        private void DrawEquipmentRow(XGraphics gfx, CIJEquipmentViewModel item, double[] widths, ref double y)
        {
            double x = MarginLeft;

            double height = 25;

            string[] values =
            {
        item.SlNo.ToString(),
        item.EquipmentName,
        item.Make,
        item.Model,
        item.EquipmentQty.ToString(),
        FormatCurrency(item.EquipmentCost)
    };

            var border =
                new XPen(XColors.LightGray, 0.7);

            for (int i = 0; i < values.Length; i++)
            {
                gfx.DrawRectangle(
                    XBrushes.White,
                    x,
                    y,
                    widths[i],
                    height);

                gfx.DrawRectangle(
                    border,
                    x,
                    y,
                    widths[i],
                    height);

                XStringFormat format =
                    i == 1 || i == 2 || i == 3
                        ? XStringFormats.TopLeft
                        : XStringFormats.TopCenter;

                gfx.DrawString(
                    values[i] ?? "",
                    _normalFont,
                    XBrushes.Black,
                    new XRect(
                        x + 4,
                        y + 6,
                        widths[i] - 8,
                        height),
                    format);

                x += widths[i];
            }

            y += height;
        }
        private string FormatCurrency(decimal amount)
        {
            return $"₹ {amount:N0}";
        }
        private void DrawTotalRow(XGraphics gfx, decimal total, double[] widths, ref double y)
        {
            double x = MarginLeft;

            double height = 28;

            // First 5 columns
            double labelWidth = widths.Take(5).Sum();

            // TOTAL label
            gfx.DrawRectangle(
                new XSolidBrush(
                    XColor.FromArgb(242, 242, 242)),
                x,
                y,
                labelWidth,
                height);

            gfx.DrawRectangle(
                XPens.Gray,
                x,
                y,
                labelWidth,
                height);

            gfx.DrawString(
                "TOTAL EQUIPMENT COST",
                _boldFont,
                XBrushes.Black,
                new XRect(
                    x + 5,
                    y + 7,
                    labelWidth - 10,
                    height),
                XStringFormats.TopRight);

            // Move to 6th column
            x += labelWidth;

            // Total cost
            gfx.DrawRectangle(
                new XSolidBrush(
                    XColor.FromArgb(221, 235, 247)),
                x,
                y,
                widths[5],
                height);

            gfx.DrawRectangle(
                XPens.Gray,
                x,
                y,
                widths[5],
                height);

            gfx.DrawString(
                FormatCurrency(total),
                _boldFont,
                XBrushes.Black,
                new XRect(
                    x + 4,
                    y + 7,
                    widths[5] - 8,
                    height),
                XStringFormats.TopRight);

            y += height + 15;
        }
        private void DrawJustification(PdfDocument document, XGraphics gfx, PdfPage page, CIJPdfViewModel model, ref double y, ref int pageNumber)
        {
            y += 5;

            DrawSectionTitle(
                gfx,
                "JUSTIFICATION",
                ref y);

            string text =
                string.IsNullOrWhiteSpace(model.cIJJustificationViewModel.Justification)
                    ? "No justification provided."
                    : model.cIJJustificationViewModel.Justification;

            var lines =
                WrapText(
                    gfx,
                    text,
                    _normalFont,
                    PageWidth - MarginLeft - MarginRight - 20);

            foreach (var line in lines)
            {
                if (y > PageHeight - 80)
                {
                    //DrawFooter(gfx, page, model, pageNumber);

                    page = document.AddPage();

                    gfx = XGraphics.FromPdfPage(page);
                    pageNumber++;
                    y = MarginTop;

                    DrawSectionTitle(
                        gfx,
                        "JUSTIFICATION - CONTINUED",
                        ref y);
                }

                gfx.DrawString(
                    line,
                    _normalFont,
                    XBrushes.Black,
                    new XRect(
                        MarginLeft + 5,
                        y,
                        PageWidth - MarginLeft - MarginRight - 10,
                        18),
                    XStringFormats.TopLeft);

                y += 16;
            }

            y += 10;
        }
        private List<string> WrapText(XGraphics gfx, string text, XFont font, double maxWidth)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
                return result;

            string[] paragraphs =
                text.Replace("\r\n", "\n")
                    .Split('\n');

            foreach (var paragraph in paragraphs)
            {
                if (string.IsNullOrWhiteSpace(paragraph))
                {
                    result.Add(string.Empty);
                    continue;
                }

                string[] words =
                    paragraph.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries);

                string currentLine = "";

                foreach (var word in words)
                {
                    string testLine =
                        string.IsNullOrEmpty(currentLine)
                            ? word
                            : currentLine + " " + word;

                    double width =
                        gfx.MeasureString(
                            testLine,
                            font).Width;

                    if (width <= maxWidth)
                    {
                        currentLine = testLine;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(currentLine))
                            result.Add(currentLine);

                        currentLine = word;
                    }
                }

                if (!string.IsNullOrEmpty(currentLine))
                    result.Add(currentLine);
            }

            return result;
        }
        private void DrawApprovalHistory(PdfDocument document, XGraphics gfx, PdfPage page, CIJPdfViewModel model, ref double y, ref int pageNumber)
        {
            if (model.workflowHistory == null ||
                model.workflowHistory.Count == 0)
                return;

            y += 10;

            DrawSectionTitle(
                gfx,
                "APPROVAL HISTORY",
                ref y);

            double[] widths =
    {
        80,   // Approved Date
        100,  // Approved By
        80,   // Role
        185,  // Remarks
        70    // Status
    };

            string[] headers =
            {
        "Approved Date",
        "Approved By",
        "Role",
        "Remarks",
        "Status",
    };

            DrawTableHeader(
                gfx,
                headers,
                widths,
                ref y);

            foreach (var item in model.workflowHistory)
            {
                if (y > PageHeight - 100)
                {
                    //DrawFooter(gfx, page, model, pageNumber);

                    page = document.AddPage();

                    page.Size =
                        PdfSharp.PageSize.A4;

                    gfx =
                        XGraphics.FromPdfPage(page);
                    pageNumber++;
                    y = MarginTop;

                    DrawSectionTitle(
                        gfx,
                        "APPROVAL HISTORY - CONTINUED",
                        ref y);

                    DrawTableHeader(
                        gfx,
                        headers,
                        widths,
                        ref y);
                }

                DrawApprovalRow(
                    gfx,
                    item,
                    widths,
                    ref y);
            }

            y += 10;
        }
        private void DrawApprovalRow(XGraphics gfx, WorkflowHistoryViewModel item, double[] widths, ref double y)
        {
            double x = MarginLeft;

            double height = 35;

            string[] values =
            {
        item.ActionDate?.ToString("dd-MMM-yyyy hh:mm") ?? "",
        item.UserName,
        item.RoleName,
        item.Remark,
        item.StatusName
    };

            var border =
                new XPen(XColors.LightGray, 0.7);

            for (int i = 0; i < values.Length; i++)
            {
                gfx.DrawRectangle(
                    XBrushes.White,
                    x,
                    y,
                    widths[i],
                    height);

                gfx.DrawRectangle(
                    border,
                    x,
                    y,
                    widths[i],
                    height);

                gfx.DrawString(
                    values[i] ?? "",
                    _smallFont,
                    XBrushes.Black,
                    new XRect(
                        x + 4,
                        y + 5,
                        widths[i] - 8,
                        height - 8),
                    XStringFormats.TopLeft);

                x += widths[i];
            }

            y += height;
        }
        private void DrawFooter(XGraphics gfx, PdfPage page, CIJPdfViewModel model, int pageNumber)
        {
            double footerY =
                PageHeight - MarginBottom - 25;

            double contentWidth =
                PageWidth - MarginLeft - MarginRight;

            // Footer separator
            gfx.DrawLine(
                new XPen(
                    XColors.LightGray,
                    0.7),
                MarginLeft,
                footerY,
                PageWidth - MarginRight,
                footerY);

            // CIJ Number - Left
            gfx.DrawString(
                $"CIJ No: {model.CIJRequest.CIJSNumber}",
                _smallFont,
                XBrushes.Gray,
                new XRect(
                    MarginLeft,
                    footerY + 6,
                    contentWidth / 3,
                    15),
                XStringFormats.TopLeft);

            // Generated text - Center
            gfx.DrawString(
                "Generated by CIJ System",
                _smallFont,
                XBrushes.Gray,
                new XRect(
                    MarginLeft + contentWidth / 3,
                    footerY + 6,
                    contentWidth / 3,
                    15),
                XStringFormats.TopCenter);

            // Page number - Right
            gfx.DrawString(
                $"Page {pageNumber}",
                _smallFont,
                XBrushes.Gray,
                new XRect(
                    MarginLeft + (contentWidth * 2 / 3),
                    footerY + 6,
                    contentWidth / 3,
                    15),
                XStringFormats.TopRight);
        }
    }
}
